using MediatR;

namespace Aula.Server.Common.Gateway;

internal sealed record HelloEvent : INotification
{
	public required GatewaySession Session { get; init; }
}
