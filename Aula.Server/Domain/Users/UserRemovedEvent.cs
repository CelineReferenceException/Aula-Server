namespace Aula.Server.Domain.Users;

internal sealed record UserRemovedEvent(User User) : DomainEvent;
