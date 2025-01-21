namespace WhiteTale.Server.Domain.Rooms;

internal sealed record RoomConnectionRemovedEvent(RoomConnection Connection) : DomainEvent;
