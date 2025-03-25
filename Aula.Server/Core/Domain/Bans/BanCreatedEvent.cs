namespace Aula.Server.Core.Domain.Bans;

internal sealed record BanCreatedEvent(Ban Ban) : DomainEvent;
