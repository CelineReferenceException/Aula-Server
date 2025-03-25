using Aula.Server.Domain;
using MediatR;

namespace Aula.Server.Core.Api.Rooms;

internal sealed record UserStartedTypingEvent : INotification
{
	public required Snowflake UserId { get; init; }

	public required Snowflake RoomId { get; init; }
}
