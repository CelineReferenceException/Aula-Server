using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WhiteTale.Server.Features.Rooms.Messages;

namespace WhiteTale.Server.Features.Gateway.Events.Send.Messages;

internal sealed class MessageRemovedEventHandler : INotificationHandler<MessageRemovedEvent>
{
	private readonly ApplicationDbContext _dbContext;
	private readonly JsonSerializerOptions _jsonSerializerOptions;

	public MessageRemovedEventHandler(IOptions<JsonOptions> jsonOptions, ApplicationDbContext dbContext)
	{
		_jsonSerializerOptions = jsonOptions.Value.SerializerOptions;
		_dbContext = dbContext;
	}

	public async Task Handle(MessageRemovedEvent notification, CancellationToken cancellationToken)
	{
		var operations = new List<Task>();

		var message = notification.Message;
		var payload = new GatewayPayload<MessageData>
		{
			Operation = OperationType.Dispatch,
			Event = EventType.MessageRemoved,
			Data = new MessageData
			{
				Id = message.Id,
				Type = message.Type,
				Flags = message.Flags,
				AuthorType = message.AuthorType,
				AuthorId = message.AuthorId,
				TargetType = message.TargetType,
				TargetId = message.TargetId,
				Content = message.Content,
				JoinData = message.JoinData is not null
					? new MessageUserJoinData
					{
						UserId = message.JoinData.UserId,
					}
					: null,
				LeaveData = message.LeaveData is not null
					? new MessageUserLeaveData
					{
						UserId = message.LeaveData.UserId,
						RoomId = message.LeaveData.RoomId,
					}
					: null,
				CreationTime = message.CreationTime,
			},
		};
		var payloadBytes = JsonSerializer.SerializeToUtf8Bytes(payload, _jsonSerializerOptions);

		var sessionUserIds = ConnectToGateway.Sessions.Values
			.Select(connection => connection.UserId);

		var usersCurrentRoomId = await _dbContext.Users
			.Where(u => sessionUserIds.Contains(u.Id))
			.Select(u => new
			{
				u.Id,
				u.CurrentRoomId,
			})
			.ToDictionaryAsync(u => u.Id, u => u.CurrentRoomId, cancellationToken);

		foreach (var connection in ConnectToGateway.Sessions.Values)
		{
			if (!connection.Intents.HasFlag(Intents.Messages))
			{
				continue;
			}

			var userCurrentRoomId = usersCurrentRoomId[connection.UserId];
			if (userCurrentRoomId is null ||
			    userCurrentRoomId != message.TargetId)
			{
				continue;
			}

			var operation = connection.QueueEventAsync(payloadBytes, cancellationToken);
			operations.Add(operation);
		}

		await Task.WhenAll(operations);
	}
}
