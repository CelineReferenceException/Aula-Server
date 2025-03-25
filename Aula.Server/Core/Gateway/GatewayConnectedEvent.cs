using Aula.Server.Core.Api.Users;
using MediatR;

namespace Aula.Server.Core.Gateway;

internal sealed class GatewayConnectedEvent : INotification
{
	internal required GatewaySession Session { get; init; }

	internal required PresenceOptions Presence { get; init; }
}
