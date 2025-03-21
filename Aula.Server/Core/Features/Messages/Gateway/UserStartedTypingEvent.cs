using MediatR;

namespace Aula.Server.Core.Features.Messages.Gateway;

internal sealed record UserStartedTypingEvent : INotification
{
	public required UInt64 UserId { get; init; }

	public required UInt64 RoomId { get; init; }
}
