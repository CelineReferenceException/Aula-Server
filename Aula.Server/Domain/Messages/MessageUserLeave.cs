namespace Aula.Server.Domain.Messages;

internal sealed record MessageUserLeave
{
	internal MessageUserLeave(Snowflake messageId, Snowflake userId, Snowflake? roomId)
	{
		MessageId = messageId;
		UserId = userId;
		RoomId = roomId;
	}

	internal Snowflake MessageId { get; }

	internal Snowflake UserId { get; }

	internal Snowflake? RoomId { get; }

	// Navigation property, values are set through reflection.
	internal Message? Message { get; init; }
}
