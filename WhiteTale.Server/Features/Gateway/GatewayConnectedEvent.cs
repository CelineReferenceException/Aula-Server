using MediatR;
using WhiteTale.Server.Features.Gateway.Events.Presences;

namespace WhiteTale.Server.Features.Gateway;

internal sealed class GatewayConnectedEvent : INotification
{
	internal required GatewaySession Session { get; init; }

	internal required PresenceOptions Presence { get; init; }
}
