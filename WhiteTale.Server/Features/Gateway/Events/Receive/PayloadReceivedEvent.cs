using MediatR;

namespace WhiteTale.Server.Features.Gateway.Events.Receive;

internal sealed record PayloadReceivedEvent : INotification
{
	public required GatewaySession Session { get; init; }

	public required GatewayPayload Payload { get; init; }
}
