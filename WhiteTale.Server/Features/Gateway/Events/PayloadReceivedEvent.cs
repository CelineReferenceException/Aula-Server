using MediatR;

namespace WhiteTale.Server.Features.Gateway.Events;

internal sealed record PayloadReceivedEvent : INotification
{
	public required GatewaySession Session { get; init; }

	public required GatewayPayload Payload { get; init; }
}
