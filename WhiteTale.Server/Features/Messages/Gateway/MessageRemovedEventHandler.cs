using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using EventType = WhiteTale.Server.Common.Gateway.EventType;
using Intents = WhiteTale.Server.Common.Gateway.Intents;
using OperationType = WhiteTale.Server.Common.Gateway.OperationType;

namespace WhiteTale.Server.Features.Messages.Gateway;

internal sealed class MessageRemovedEventHandler : INotificationHandler<MessageRemovedEvent>
{
	private readonly ApplicationDbContext _dbContext;
	private readonly JsonSerializerOptions _jsonSerializerOptions;
	private readonly GatewayService _gatewayService;

	public MessageRemovedEventHandler(IOptions<JsonOptions> jsonOptions, ApplicationDbContext dbContext, GatewayService gatewayService)
	{
		_jsonSerializerOptions = jsonOptions.Value.SerializerOptions;
		_dbContext = dbContext;
		_gatewayService = gatewayService;
	}

	public async Task Handle(MessageRemovedEvent notification, CancellationToken cancellationToken)
	{
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
		}.GetJsonUtf8Bytes(_jsonSerializerOptions);

		var sessionUserIds = _gatewayService.Sessions.Values
			.Select(connection => connection.UserId);

		var usersCurrentRoomId = await _dbContext.Users
			.Where(u => sessionUserIds.Contains(u.Id))
			.Select(u => new
			{
				u.Id,
				u.CurrentRoomId,
			})
			.ToDictionaryAsync(u => u.Id, u => u.CurrentRoomId, cancellationToken);

		foreach (var connection in _gatewayService.Sessions.Values)
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

			_ = connection.QueueEventAsync(payload, cancellationToken);
		}
	}
}
