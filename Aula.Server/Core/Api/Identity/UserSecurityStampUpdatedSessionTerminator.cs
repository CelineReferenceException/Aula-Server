using System.Net.WebSockets;
using Aula.Server.Domain.Users;
using MediatR;

namespace Aula.Server.Core.Api.Identity;

internal sealed class UserSecurityStampUpdatedSessionTerminator : INotificationHandler<UserSecurityStampUpdatedEvent>
{
	private readonly GatewaySessionManager _gatewaySessionManager;

	public UserSecurityStampUpdatedSessionTerminator(GatewaySessionManager gatewaySessionManager)
	{
		_gatewaySessionManager = gatewaySessionManager;
	}

	public Task Handle(UserSecurityStampUpdatedEvent notification, CancellationToken cancellationToken)
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
