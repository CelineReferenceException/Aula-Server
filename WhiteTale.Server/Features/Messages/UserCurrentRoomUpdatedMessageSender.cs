using MediatR;

namespace WhiteTale.Server.Features.Messages;

internal sealed class UserCurrentRoomUpdatedMessageSender : INotificationHandler<UserCurrentRoomUpdatedEvent>
{
	private readonly ApplicationDbContext _dbContext;
	private readonly SnowflakeProvider _snowflakeProvider;

	public UserCurrentRoomUpdatedMessageSender(
		ApplicationDbContext dbContext,
		SnowflakeProvider snowflakeProvider)
	{
		_dbContext = dbContext;
		_snowflakeProvider = snowflakeProvider;
	}

	public async Task Handle(UserCurrentRoomUpdatedEvent notification, CancellationToken cancellationToken)
	{
		if (notification.PreviousRoomId is not null)
		{
			var leaveMessageId = _snowflakeProvider.NewSnowflake();
			var leaveMessage = Message.Create(leaveMessageId, MessageType.UserLeave, 0, MessageAuthor.System, null,
				MessageTarget.Room, null, null, new MessageUserLeave
				{
					Id = leaveMessageId,
					Message = null!,
					UserId = notification.UserId,
					RoomId = notification.CurrentRoomId,
				}, notification.PreviousRoomId.Value);

			_ = _dbContext.Messages.Add(leaveMessage);
		}

		if (notification.CurrentRoomId is not null)
		{
			var joinMessageId = _snowflakeProvider.NewSnowflake();
			var joinMessage = Message.Create(joinMessageId, MessageType.UserJoin, 0, MessageAuthor.System, null,
				MessageTarget.Room, null, new MessageUserJoin
				{
					Id = joinMessageId,
					Message = null!,
					UserId = notification.UserId,
				}, null, notification.CurrentRoomId.Value);

			_ = _dbContext.Messages.Add(joinMessage);
		}

		_ = await _dbContext.SaveChangesAsync(cancellationToken);
	}
}
