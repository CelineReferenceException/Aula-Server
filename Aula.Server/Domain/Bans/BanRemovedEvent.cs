namespace Aula.Server.Domain.Bans;

internal sealed record BanRemovedEvent(Ban Ban) : DomainEvent;
