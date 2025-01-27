namespace WhiteTale.Server.Domain.Messages;

internal sealed record MessageUserJoin
{
	internal required UInt64 Id { get; init; }

	internal required Message Message { get; init; }

	internal required UInt64 UserId { get; init; }
}
