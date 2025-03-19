using System.Net.WebSockets;
using MediatR;

namespace Aula.Server.Features.Identity.Gateway;

internal sealed class UserSecurityStampUpdatedGatewaySessionTerminator : INotificationHandler<UserSecurityStampUpdatedEvent>
{
	private readonly GatewayService _gatewayService;

	public UserSecurityStampUpdatedGatewaySessionTerminator(GatewayService gatewayService)
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
