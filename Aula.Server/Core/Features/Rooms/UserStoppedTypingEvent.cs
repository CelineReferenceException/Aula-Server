using Aula.Server.Core.Domain;
using MediatR;

namespace Aula.Server.Core.Features.Rooms;

internal sealed record UserStoppedTypingEvent : INotification
{
	public required Snowflake UserId { get; init; }

	public required Snowflake RoomId { get; init; }
}
