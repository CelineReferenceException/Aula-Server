namespace WhiteTale.Server.Domain.Characters;

internal sealed record CharacterCurrentRoomUpdatedEvent(UInt64 CharacterId, UInt64? RoomId) : DomainEvent;
