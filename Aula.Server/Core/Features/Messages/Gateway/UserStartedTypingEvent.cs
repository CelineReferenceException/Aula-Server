using MediatR;

namespace Aula.Server.Core.Features.Messages.Gateway;

internal sealed record UserStartedTypingEvent : INotification
{
	public required Snowflake UserId { get; init; }

	public required Snowflake RoomId { get; init; }
}
