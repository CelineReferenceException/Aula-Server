namespace WhiteTale.Server.Domain.Users;

internal sealed record UserUpdatedEvent(User User) : DomainEvent;
