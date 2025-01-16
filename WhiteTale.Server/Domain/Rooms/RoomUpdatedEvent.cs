namespace WhiteTale.Server.Domain.Rooms;

internal sealed record RoomUpdatedEvent(Room Room) : DomainEvent;
