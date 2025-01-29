namespace WhiteTale.Server.Domain.Bans;

internal sealed record BanRemovedEvent(Ban Ban) : DomainEvent;
