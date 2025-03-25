namespace Aula.Server.Core.Domain.Messages;

internal sealed record MessageCreatedEvent(Message Message) : DomainEvent;
