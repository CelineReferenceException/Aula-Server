using MediatR;

namespace Aula.Server.Common.Gateway;

internal sealed class GatewayDisconnectedEvent : INotification
{
	internal required GatewaySession Session { get; init; }
}
