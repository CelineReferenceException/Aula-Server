﻿namespace Aula.Server.Domain.Bans;

internal sealed record BanCreatedEvent(Ban Ban) : DomainEvent;
