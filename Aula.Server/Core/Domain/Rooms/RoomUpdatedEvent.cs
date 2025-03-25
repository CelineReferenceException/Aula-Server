namespace Aula.Server.Core.Domain.Rooms;

internal sealed record RoomUpdatedEvent(Room Room) : DomainEvent;
