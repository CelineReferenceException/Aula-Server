namespace Aula.Server.Core.Domain.Rooms;

internal sealed record RoomConnectionCreatedEvent(RoomConnection Connection) : DomainEvent;
