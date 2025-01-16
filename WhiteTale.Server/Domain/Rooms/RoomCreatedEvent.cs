namespace WhiteTale.Server.Domain.Rooms;

internal sealed record RoomCreatedEvent(Room Room) : DomainEvent;
