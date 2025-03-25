namespace Aula.Server.Core.Domain.Bans;

internal sealed record BanRemovedEvent(Ban Ban) : DomainEvent;
