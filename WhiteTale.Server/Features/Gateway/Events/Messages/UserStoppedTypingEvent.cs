using MediatR;

namespace WhiteTale.Server.Features.Gateway.Events.Messages;

internal sealed record UserStoppedTypingEvent : INotification
{
	public required UInt64 UserId { get; init; }

	public required UInt64 RoomId { get; init; }
}
