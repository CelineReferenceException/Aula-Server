using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text.Json;
using Aula.Server.Common.Persistence;
using Aula.Server.Common.Resilience;
using Aula.Server.Domain;
using Aula.Server.Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Polly;

namespace Aula.Server.Core.Api.Users;

internal sealed class PresenceUpdater :
	INotificationHandler<GatewayConnectedEvent>,
	INotificationHandler<PayloadReceivedEvent>,
	INotificationHandler<GatewayDisconnectedEvent>
{
	private static readonly ConcurrentDictionary<Snowflake, UserPresenceState> s_presenceStates = new();
	private readonly ApplicationDbContext _dbContext;
	private readonly JsonSerializerOptions _jsonSerializerOptions;
	private readonly ResiliencePipeline _retryOnDbConcurrencyProblem;

	public PresenceUpdater(
		IOptions<JsonOptions> jsonOptions,
		ApplicationDbContext dbContext,
		[FromKeyedServices(ResiliencePipelines.RetryOnDbConcurrencyProblem)] ResiliencePipeline retryOnDbConcurrencyProblem)
	{
		_jsonSerializerOptions = jsonOptions.Value.SerializerOptions;
		_dbContext = dbContext;
		_retryOnDbConcurrencyProblem = retryOnDbConcurrencyProblem;
	}

	public async Task Handle(GatewayConnectedEvent notification, CancellationToken cancellationToken)
	{
		var session = notification.Session;
		var presenceState = s_presenceStates.GetOrAdd(session.UserId, _ => new UserPresenceState());

		await presenceState.UpdateSemaphore.WaitAsync(cancellationToken);

		presenceState.GatewayCount++;

		await _retryOnDbConcurrencyProblem.ExecuteAsync(async ct =>
		{
			var user = await _dbContext.Users
				.AsTracking()
				.Where(u => u.Id == session.UserId)
				.FirstOrDefaultAsync(ct) ?? throw new UnreachableException("User expected to exist");

			user.Modify(presence: GetPresence(notification.Presence)).ThrowIfFailed();
			user.UpdateConcurrencyStamp();

			_ = await _dbContext.SaveChangesAsync(ct);
			_ = presenceState.UpdateSemaphore.Release();
		}, cancellationToken);
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

		await _retryOnDbConcurrencyProblem.ExecuteAsync(async ct =>
		{
			var user = await _dbContext.Users
				.AsTracking()
				.Where(u => u.Id == notification.Session.UserId)
				.FirstOrDefaultAsync(ct) ?? throw new UnreachableException("User expected to exist");

			user.Modify(presence: Presence.Offline).ThrowIfFailed();
			user.UpdateConcurrencyStamp();

			_ = await _dbContext.SaveChangesAsync(ct);
			_ = presenceState.UpdateSemaphore.Release();
		}, cancellationToken);
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

		await _retryOnDbConcurrencyProblem.ExecuteAsync(async ct =>
		{
			var user = await _dbContext.Users
				.AsTracking()
				.Where(u => u.Id == session.UserId)
				.FirstOrDefaultAsync(ct) ?? throw new UnreachableException("User expected to exist");

			user.Modify(presence: GetPresence(data.Presence)).ThrowIfFailed();
			user.UpdateConcurrencyStamp();

			_ = await _dbContext.SaveChangesAsync(ct);
			_ = presenceState.UpdateSemaphore.Release();
		}, cancellationToken);
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
