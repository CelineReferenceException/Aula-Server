using MediatR;
using WhiteTale.Server.Features.Users.Gateway;

namespace WhiteTale.Server.Common.Gateway;

internal sealed class GatewayConnectedEvent : INotification
{
	internal required GatewaySession Session { get; init; }

	internal required PresenceOptions Presence { get; init; }
}
