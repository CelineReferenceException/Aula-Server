namespace Aula.Server.Core.Domain.Users;

internal sealed record UserUpdatedEvent(User User) : DomainEvent;
