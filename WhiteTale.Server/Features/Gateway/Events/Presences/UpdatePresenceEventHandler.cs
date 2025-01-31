using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace WhiteTale.Server.Features.Gateway.Events.Presences;

internal sealed class UpdatePresenceEventHandler :
	INotificationHandler<GatewayConnectedEvent>,
	INotificationHandler<PayloadReceivedEvent>,
	INotificationHandler<GatewayDisconnectedEvent>
{
	private static readonly ConcurrentDictionary<UInt64, UserPresenceState> s_presenceStates = new();
	private readonly JsonSerializerOptions _jsonSerializerOptions;
	private readonly ApplicationDbContext _dbContext;

	public UpdatePresenceEventHandler(IOptions<JsonOptions> jsonOptions, ApplicationDbContext dbContext)
	{
		_jsonSerializerOptions = jsonOptions.Value.SerializerOptions;
		_dbContext = dbContext;
	}

	public async Task Handle(GatewayConnectedEvent notification, CancellationToken cancellationToken)
	{
		var session = notification.Session;
		var presenceState = s_presenceStates.GetOrAdd(session.UserId, _ => new UserPresenceState());

		await presenceState.UpdateSemaphore.WaitAsync(cancellationToken);

		presenceState.GatewayCount++;

		var user = await _dbContext.Users
			.Where(u => u.Id == session.UserId)
			.FirstOrDefaultAsync(cancellationToken) ?? throw new UnreachableException("User expected to exist");

		user.Modify(presence: GetPresence(notification.Presence));
		user.UpdateConcurrencyStamp();

		_ = await _dbContext.SaveChangesAsync(cancellationToken);
		_ = presenceState.UpdateSemaphore.Release();
	}

	public async Task Handle(GatewayDisconnectedEvent notification, CancellationToken cancellationToken)
	{
		var session = notification.Session;
		if (!s_presenceStates.TryGetValue(session.UserId, out var presenceState))
		{
			throw new UnreachableException("Expected gateway state to be traced.");
		}

		await presenceState.UpdateSemaphore.WaitAsync(cancellationToken);

		if (presenceState.GatewayCount > 1)
		{
			presenceState.GatewayCount--;
		}
		else
		{
			_ = s_presenceStates.TryRemove(session.UserId, out _);
		}

		var user = await _dbContext.Users
			.Where(u => u.Id == notification.Session.UserId)
			.FirstOrDefaultAsync(cancellationToken) ?? throw new UnreachableException("User expected to exist");

		user.Modify(presence: Presence.Offline);
		user.UpdateConcurrencyStamp();

		_ = await _dbContext.SaveChangesAsync(cancellationToken);
		_ = presenceState.UpdateSemaphore.Release();
	}

	public async Task Handle(PayloadReceivedEvent notification, CancellationToken cancellationToken)
	{
		var payload = notification.Payload;
		if (payload.Operation is not OperationType.Dispatch ||
		    payload.Event is not EventType.UpdatePresence)
		{
			return;
		}

		var session = notification.Session;
		UpdatePresenceEventData data;
		try
		{
			data = payload.Data.Deserialize<UpdatePresenceEventData>(_jsonSerializerOptions) ??
			       throw new JsonException("Data expected to not be null");
		}
		catch (JsonException)
		{
			await session.StopAsync(WebSocketCloseStatus.InvalidPayloadData);
			return;
		}

		if (!s_presenceStates.TryGetValue(session.UserId, out var presenceState))
		{
			throw new UnreachableException("Expected gateway state to be traced.");
		}

		await presenceState.UpdateSemaphore.WaitAsync(cancellationToken);

		var user = await _dbContext.Users
			.Where(u => u.Id == session.UserId)
			.FirstOrDefaultAsync(cancellationToken) ?? throw new UnreachableException("User expected to exist");

		user.Modify(presence: GetPresence(data.Presence));
		user.UpdateConcurrencyStamp();

		_ = await _dbContext.SaveChangesAsync(cancellationToken);
		_ = presenceState.UpdateSemaphore.Release();
	}

	private static Presence GetPresence(PresenceOptions presenceOptions)
	{
		return presenceOptions switch
		{
			PresenceOptions.Invisible => Presence.Offline,
			PresenceOptions.Online => Presence.Online,
			_ => throw new InvalidOperationException($"Unhandled {nameof(PresenceOptions)} case: {presenceOptions})"),
		};
	}

	private sealed class UserPresenceState
	{
		internal Int32 GatewayCount { get; set; }

		internal SemaphoreSlim UpdateSemaphore { get; } = new(1, 1);
	}
}
