﻿using Aula.Server.Core.Persistence;
using MediatR;

namespace Aula.Server.Core.Features.Messages;

internal sealed class UserCurrentRoomUpdatedMessageSender : INotificationHandler<UserCurrentRoomUpdatedEvent>
{
	private readonly ApplicationDbContext _dbContext;
	private readonly SnowflakeGenerator _snowflakeGenerator;

	public UserCurrentRoomUpdatedMessageSender(
		ApplicationDbContext dbContext,
		SnowflakeGenerator snowflakeGenerator)
	{
		_dbContext = dbContext;
		_snowflakeGenerator = snowflakeGenerator;
	}

	public async Task Handle(UserCurrentRoomUpdatedEvent notification, CancellationToken cancellationToken)
	{
		if (notification.PreviousRoomId is not null)
		{
			var leaveMessageId = await _snowflakeGenerator.NewSnowflakeAsync();
			var leaveMessage = new Message(leaveMessageId, MessageType.UserLeave, 0, MessageAuthorType.System, null, null,
				notification.PreviousRoomId.Value)
			{
				LeaveData = new MessageUserLeave(leaveMessageId, notification.UserId, notification.CurrentRoomId),
			};

			_ = _dbContext.Messages.Add(leaveMessage);
		}

		if (notification.CurrentRoomId is not null)
		{
			var joinMessageId = await _snowflakeGenerator.NewSnowflakeAsync();
			var joinMessage = new Message(joinMessageId, MessageType.UserJoin, 0, MessageAuthorType.System, null, null,
				notification.CurrentRoomId.Value)
			{
				JoinData = new MessageUserJoin(joinMessageId, notification.UserId),
			};

			_ = _dbContext.Messages.Add(joinMessage);
		}

		_ = await _dbContext.SaveChangesAsync(cancellationToken);
	}
}
