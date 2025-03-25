using Aula.Server.Core.Api.Users;
using MediatR;

namespace Aula.Server.Common.Gateway;

internal sealed class GatewayConnectedEvent : INotification
{
	internal required GatewaySession Session { get; init; }

	internal required PresenceOptions Presence { get; init; }
}
