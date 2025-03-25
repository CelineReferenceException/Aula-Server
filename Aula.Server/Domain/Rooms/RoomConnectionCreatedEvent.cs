namespace Aula.Server.Domain.Rooms;

internal sealed record RoomConnectionCreatedEvent(RoomConnection Connection) : DomainEvent;
