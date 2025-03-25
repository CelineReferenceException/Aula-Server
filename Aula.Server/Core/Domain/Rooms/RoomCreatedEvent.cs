namespace Aula.Server.Core.Domain.Rooms;

internal sealed record RoomCreatedEvent(Room Room) : DomainEvent;
