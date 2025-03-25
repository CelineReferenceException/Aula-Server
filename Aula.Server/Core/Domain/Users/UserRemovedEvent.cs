namespace Aula.Server.Core.Domain.Users;

internal sealed record UserRemovedEvent(User User) : DomainEvent;
