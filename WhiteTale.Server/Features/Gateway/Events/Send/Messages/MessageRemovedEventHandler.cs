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
				AuthorId = message.AuthorId,
				Target = message.Target,
				TargetId = message.TargetId,
				Content = message.Content,
				CreationTime = message.CreationTime,
			},
		};
		var payloadBytes = JsonSerializer.SerializeToUtf8Bytes(payload, _jsonSerializerOptions);

		var sessionUserIds = ConnectToGateway.Sessions.Values
			.Select(connection => connection.UserId);

		var usersCurrentRoomId = await _dbContext.Users
			.Where(user => sessionUserIds.Contains(user.Id))
			.Select(user => new
			{
				user.Id,
				user.CurrentRoomId,
			})
			.ToDictionaryAsync(user => user.Id, user => user.CurrentRoomId, cancellationToken);

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
