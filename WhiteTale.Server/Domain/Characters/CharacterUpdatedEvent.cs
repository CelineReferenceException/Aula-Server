namespace WhiteTale.Server.Domain.Characters;

internal sealed record CharacterUpdatedEvent(Character Character) : DomainEvent;
