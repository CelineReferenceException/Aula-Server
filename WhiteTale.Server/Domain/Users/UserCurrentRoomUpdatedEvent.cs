namespace WhiteTale.Server.Domain.Users;

internal sealed record UserCurrentRoomUpdatedEvent(UInt64 UserId, UInt64? RoomId) : DomainEvent;
