using MediatR;

namespace Aula.Server.Features.Messages;

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
			var leaveMessageId = _snowflakeGenerator.NewSnowflakeAsync();
			var leaveMessage = Message.Create(leaveMessageId, MessageType.UserLeave, 0, MessageAuthorType.System, null, null, null,
				new MessageUserLeave
				{
					MessageId = leaveMessageId,
					UserId = notification.UserId,
					RoomId = notification.CurrentRoomId,
				}, notification.PreviousRoomId.Value);

			_ = _dbContext.Messages.Add(leaveMessage);
		}

		if (notification.CurrentRoomId is not null)
		{
			var joinMessageId = _snowflakeGenerator.NewSnowflakeAsync();
			var joinMessage = Message.Create(joinMessageId, MessageType.UserJoin, 0, MessageAuthorType.System, null, null,
				new MessageUserJoin
				{
					MessageId = joinMessageId,
					UserId = notification.UserId,
				}, null, notification.CurrentRoomId.Value);

			_ = _dbContext.Messages.Add(joinMessage);
		}

		_ = await _dbContext.SaveChangesAsync(cancellationToken);
	}
}
