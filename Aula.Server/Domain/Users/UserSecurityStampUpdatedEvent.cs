namespace Aula.Server.Domain.Users;

internal sealed record UserSecurityStampUpdatedEvent(User User) : DomainEvent;
