namespace Aula.Server.Core.Domain.Users;

internal sealed record UserCurrentRoomUpdatedEvent(Snowflake UserId, Snowflake? PreviousRoomId, Snowflake? CurrentRoomId) : DomainEvent;
