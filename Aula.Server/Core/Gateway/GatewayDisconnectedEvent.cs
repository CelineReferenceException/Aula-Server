using MediatR;

namespace Aula.Server.Core.Gateway;

internal sealed class GatewayDisconnectedEvent : INotification
{
	internal required GatewaySession Session { get; init; }
}
