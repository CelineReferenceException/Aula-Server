using MediatR;

namespace WhiteTale.Server.Features.Gateway;

internal sealed class GatewayDisconnectedEvent : INotification
{
	internal required GatewaySession Session { get; init; }
}
