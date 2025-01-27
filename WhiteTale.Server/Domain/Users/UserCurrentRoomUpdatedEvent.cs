namespace WhiteTale.Server.Domain.Users;

internal sealed record UserCurrentRoomUpdatedEvent(UInt64 UserId, UInt64? PreviousRoomId, UInt64? CurrentRoomId) : DomainEvent;
