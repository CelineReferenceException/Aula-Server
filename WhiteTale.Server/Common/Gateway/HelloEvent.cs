using MediatR;

namespace WhiteTale.Server.Common.Gateway;

internal sealed record HelloEvent : INotification
{
	public required GatewaySession Session { get; init; }
}
