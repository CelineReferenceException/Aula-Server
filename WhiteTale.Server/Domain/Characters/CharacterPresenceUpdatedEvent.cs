namespace WhiteTale.Server.Domain.Characters;

internal sealed record CharacterPresenceUpdatedEvent(UInt64 CharacterId, Presence Presence) : DomainEvent;
