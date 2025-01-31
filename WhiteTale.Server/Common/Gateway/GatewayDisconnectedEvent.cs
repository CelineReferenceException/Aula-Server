using MediatR;

namespace WhiteTale.Server.Common.Gateway;

internal sealed class GatewayDisconnectedEvent : INotification
{
	internal required GatewaySession Session { get; init; }
}
