using Aula.Server.Core.Persistence;
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
			var leaveMessage = Message.Create(leaveMessageId, MessageType.UserLeave, 0, MessageAuthorType.System, null, null,
					notification.PreviousRoomId.Value, null,
					new MessageUserLeave(leaveMessageId, notification.UserId, notification.CurrentRoomId))
				.Value!;

			_ = _dbContext.Messages.Add(leaveMessage);
		}

		if (notification.CurrentRoomId is not null)
		{
			var joinMessageId = await _snowflakeGenerator.NewSnowflakeAsync();
			var joinMessage = Message.Create(joinMessageId, MessageType.UserJoin, 0, MessageAuthorType.System, null, null,
					notification.CurrentRoomId.Value, new MessageUserJoin(joinMessageId, notification.UserId), null)
				.Value!;

			_ = _dbContext.Messages.Add(joinMessage);
		}

		_ = await _dbContext.SaveChangesAsync(cancellationToken);
	}
}
