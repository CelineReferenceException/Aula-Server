namespace Aula.Server.Domain.Messages;

internal sealed record MessageUserJoin
{
	internal MessageUserJoin(Snowflake messageId, Snowflake userId)
	{
		MessageId = messageId;
		UserId = userId;
	}

	internal Snowflake MessageId { get; }

	// Navigation property, values are set through reflection.
	internal Message? Message { get; init; }

	internal Snowflake UserId { get; }
}
