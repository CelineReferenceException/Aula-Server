namespace WhiteTale.Server.Features.Gateway.Events.Send.Messages;

internal sealed record UserTypingEventData
{
	public required UInt64 UserId { get; init; }

	public required UInt64 RoomId { get; init; }
}
