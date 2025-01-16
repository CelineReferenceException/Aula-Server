namespace WhiteTale.Server.Domain.Rooms;

internal sealed record RoomRemovedEvent(Room Room) : DomainEvent;
