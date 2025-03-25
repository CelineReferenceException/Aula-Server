namespace Aula.Server.Core.Domain.Messages;

internal sealed record MessageUserLeave
{
	internal Snowflake MessageId { get; }

	// Navigation property, values are set through reflection.
	internal Message? Message { get; init; }

	internal Snowflake UserId { get; }

	internal Snowflake? RoomId { get; }

	internal MessageUserLeave(Snowflake messageId, Snowflake userId, Snowflake? roomId)
	{
		MessageId = messageId;
		UserId = userId;
		RoomId = roomId;
	}
}
