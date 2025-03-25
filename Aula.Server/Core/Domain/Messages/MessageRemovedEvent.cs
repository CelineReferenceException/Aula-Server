namespace Aula.Server.Core.Domain.Messages;

internal sealed record MessageRemovedEvent(Message Message) : DomainEvent;
