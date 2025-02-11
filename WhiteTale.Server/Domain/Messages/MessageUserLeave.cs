namespace WhiteTale.Server.Domain.Messages;

internal sealed record MessageUserLeave
{
	internal required UInt64 MessageId { get; init; }

	internal required Message Message { get; init; }

	internal required UInt64 UserId { get; init; }

	internal UInt64? RoomId { get; init; }
}
