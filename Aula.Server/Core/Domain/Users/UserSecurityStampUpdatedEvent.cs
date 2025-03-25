namespace Aula.Server.Core.Domain.Users;

internal sealed record UserSecurityStampUpdatedEvent(User User) : DomainEvent;
