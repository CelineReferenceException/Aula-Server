using MediatR;

namespace WhiteTale.Server.Features.Gateway.Events.Send.Hello;

internal sealed record HelloEvent : INotification
{
	public required GatewaySession Session { get; init; }
}
