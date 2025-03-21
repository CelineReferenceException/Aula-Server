using MediatR;

namespace Aula.Server.Core.Gateway;

internal sealed record GatewaySessionReadyEvent : INotification
{
	public required GatewaySession Session { get; init; }
}
