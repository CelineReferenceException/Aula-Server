namespace WhiteTale.Server.Domain.Users;

internal sealed record UserSecurityStampUpdatedEvent(User User) : DomainEvent;
