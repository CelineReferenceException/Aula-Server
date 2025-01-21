namespace WhiteTale.Server.Domain.Rooms;

internal sealed record RoomConnectionCreatedEvent(RoomConnection Connection) : DomainEvent;
