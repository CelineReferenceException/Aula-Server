namespace WhiteTale.Server.Domain.Messages;

internal sealed record MessageDeletedEvent(Message Message) : DomainEvent;
