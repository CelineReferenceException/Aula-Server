﻿using MediatR;

namespace Aula.Server.Core.Gateway;

internal sealed record PayloadReceivedEvent : INotification
{
	public required GatewaySession Session { get; init; }

	public required GatewayPayload Payload { get; init; }
}
