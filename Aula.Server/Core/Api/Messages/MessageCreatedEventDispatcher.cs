﻿using Aula.Server.Common.Persistence;
using Aula.Server.Domain.Messages;
using Aula.Server.Domain.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Core.Api.Messages;

internal sealed class MessageCreatedEventDispatcher : INotificationHandler<MessageCreatedEvent>
{
	private readonly ApplicationDbContext _dbContext;
	private readonly GatewaySessionManager _gatewaySessionManager;

	public MessageCreatedEventDispatcher(ApplicationDbContext dbContext, GatewaySessionManager gatewaySessionManager)
	{
		_dbContext = dbContext;
		_gatewaySessionManager = gatewaySessionManager;
	}

	public async Task Handle(MessageCreatedEvent notification, CancellationToken cancellationToken)
	{
		var message = notification.Message;
		var payload = new GatewayPayload<MessageData>
		{
			Operation = OperationType.Dispatch,
			Event = EventType.MessageCreated,
			Data = new MessageData
			{
				Id = message.Id,
				Type = message.Type,
				Flags = message.Flags,
				AuthorType = message.AuthorType,
				AuthorId = message.AuthorId,
				RoomId = message.RoomId,
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
				CreationDate = message.CreationDate,
			},
		};

		var sessionUserIds = _gatewaySessionManager.Sessions.Values
			.Select(session => session.UserId);

		var sessionUsers = await _dbContext.Users
			.Where(u => sessionUserIds.Contains(u.Id))
			.Select(u => new
			{
				u.Id,
				u.CurrentRoomId,
				u.Permissions,
			})
			.ToDictionaryAsync(u => u.Id, u => new
			{
				u.CurrentRoomId,
				u.Permissions,
			}, cancellationToken);

		foreach (var session in _gatewaySessionManager.Sessions.Values)
		{
			if (!session.Intents.HasFlag(Intents.Messages) ||
			    !sessionUsers.TryGetValue(session.UserId, out var user) ||
			    user.CurrentRoomId is null ||
			    !user.Permissions.HasFlag(Permissions.Administrator))
			{
				continue;
			}

			_ = session.QueueEventAsync(payload, cancellationToken);
		}
	}
}
