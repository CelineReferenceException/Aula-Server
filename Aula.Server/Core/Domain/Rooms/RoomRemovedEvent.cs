namespace Aula.Server.Core.Domain.Rooms;

internal sealed record RoomRemovedEvent(Room Room) : DomainEvent;
