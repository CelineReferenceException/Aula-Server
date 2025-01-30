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
	private static readonly ConcurrentDictionary<UInt64, UInt32> s_userGatewaysCount = new();
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

		_ = s_userGatewaysCount.AddOrUpdate(session.UserId, _ => 1, (_, count) => ++count);

		var user = await _dbContext.Users
			.Where(u => u.Id == session.UserId)
			.FirstOrDefaultAsync(cancellationToken) ?? throw new UnreachableException("User expected to exist");

		user.Modify(presence: GetPresence(notification.Presence));

		_ = await _dbContext.SaveChangesAsync(cancellationToken);
	}

	public async Task Handle(GatewayDisconnectedEvent notification, CancellationToken cancellationToken)
	{
		var session = notification.Session;
		if (!s_userGatewaysCount.TryGetValue(session.UserId, out var count))
		{
			throw new UnreachableException("Expected gateway count to be traced.");
		}

		if (count > 1)
		{
			_ = s_userGatewaysCount.TryUpdate(session.UserId, count - 1, count);
			return;
		}

		var user = await _dbContext.Users
			.Where(u => u.Id == notification.Session.UserId)
			.FirstOrDefaultAsync(cancellationToken) ?? throw new UnreachableException("User expected to exist");

		user.Modify(presence: Presence.Offline);

		_ = await _dbContext.SaveChangesAsync(cancellationToken);
		_ = s_userGatewaysCount.TryRemove(session.UserId, out _);
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

		var user = await _dbContext.Users
			.Where(u => u.Id == session.UserId)
			.FirstOrDefaultAsync(cancellationToken) ?? throw new UnreachableException("User expected to exist");

		user.Modify(presence: GetPresence(data.Presence));

		_ = await _dbContext.SaveChangesAsync(cancellationToken);

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
}
