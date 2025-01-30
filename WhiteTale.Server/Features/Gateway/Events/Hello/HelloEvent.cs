using MediatR;

namespace WhiteTale.Server.Features.Gateway.Events.Hello;

internal sealed record HelloEvent : INotification
{
	public required GatewaySession Session { get; init; }
}
