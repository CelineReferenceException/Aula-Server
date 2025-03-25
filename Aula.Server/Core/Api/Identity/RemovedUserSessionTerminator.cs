using System.Net.WebSockets;
using Aula.Server.Domain.Users;
using MediatR;

namespace Aula.Server.Core.Api.Identity;

internal sealed class RemovedUserSessionTerminator : INotificationHandler<UserRemovedEvent>
{
	private readonly GatewaySessionManager _gatewaySessionManager;

	public RemovedUserSessionTerminator(GatewaySessionManager gatewaySessionManager)
	{
		_gatewaySessionManager = gatewaySessionManager;
	}

	public Task Handle(UserRemovedEvent notification, CancellationToken cancellationToken)
	{
		var userSessions = _gatewaySessionManager.Sessions
			.Select(kvp => kvp.Value)
			.Where(s => s.UserId == notification.User.Id);

		foreach (var session in userSessions)
		{
			_ = session.StopAsync(WebSocketCloseStatus.Empty);
		}

		return Task.CompletedTask;
	}
}
