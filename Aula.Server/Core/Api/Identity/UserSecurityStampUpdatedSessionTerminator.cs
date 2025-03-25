using System.Net.WebSockets;
using Aula.Server.Core.Domain.Users;
using Aula.Server.Core.Gateway;
using MediatR;

namespace Aula.Server.Core.Api.Identity;

internal sealed class UserSecurityStampUpdatedSessionTerminator : INotificationHandler<UserSecurityStampUpdatedEvent>
{
	private readonly GatewayService _gatewayService;

	public UserSecurityStampUpdatedSessionTerminator(GatewayService gatewayService)
	{
		_gatewayService = gatewayService;
	}

	public Task Handle(UserSecurityStampUpdatedEvent notification, CancellationToken cancellationToken)
	{
		var userSessions = _gatewayService.Sessions
			.Select(kvp => kvp.Value)
			.Where(s => s.UserId == notification.User.Id);

		foreach (var session in userSessions)
		{
			_ = session.StopAsync(WebSocketCloseStatus.Empty);
		}

		return Task.CompletedTask;
	}
}
