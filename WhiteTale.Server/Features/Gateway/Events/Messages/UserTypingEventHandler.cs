using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace WhiteTale.Server.Features.Gateway.Events.Messages;

internal sealed class UserTypingEventHandler : INotificationHandler<UserTypingEvent>
{
	private readonly ApplicationDbContext _dbContext;
	private readonly JsonSerializerOptions _jsonSerializerOptions;

	public UserTypingEventHandler(IOptions<JsonOptions> jsonOptions, ApplicationDbContext dbContext)
	{
		_jsonSerializerOptions = jsonOptions.Value.SerializerOptions;
		_dbContext = dbContext;
	}


	public async Task Handle(UserTypingEvent notification, CancellationToken cancellationToken)
	{
		var operations = new List<Task>();

		var payload = new GatewayPayload<UserTypingEventData>
		{
			Operation = OperationType.Dispatch,
			Event = EventType.UserTyping,
			Data = new UserTypingEventData
			{
				UserId = notification.UserId,
				RoomId = notification.RoomId,
			},
		};
		var payloadBytes = JsonSerializer.SerializeToUtf8Bytes(payload, _jsonSerializerOptions);

		var sessionUserIds = ConnectToGateway.Sessions.Values
			.Select(session => session.UserId);

		var usersCurrentRoomId = await _dbContext.Users
			.Where(u => sessionUserIds.Contains(u.Id))
			.Select(u => new
			{
				u.Id,
				u.CurrentRoomId,
			})
			.ToDictionaryAsync(u => u.Id, u => u.CurrentRoomId, cancellationToken);

		foreach (var session in ConnectToGateway.Sessions.Values)
		{
			if (!session.Intents.HasFlag(Intents.Messages))
			{
				continue;
			}

			var userCurrentRoomId = usersCurrentRoomId[session.UserId];
			if (userCurrentRoomId is null ||
			    userCurrentRoomId != notification.RoomId)
			{
				continue;
			}

			var operation = session.QueueEventAsync(payloadBytes, cancellationToken);
			operations.Add(operation);
		}

		await Task.WhenAll(operations);
	}
}
