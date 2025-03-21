using System.Net.WebSockets;
using Aula.Server.Core.Gateway;
using MediatR;

namespace Aula.Server.Core.Features.Identity.Gateway;

internal sealed class RemovedUserSessionTerminator : INotificationHandler<UserRemovedEvent>
{
	private readonly GatewayService _gatewayService;

	public RemovedUserSessionTerminator(GatewayService gatewayService)
	{
		_gatewayService = gatewayService;
	}

	public Task Handle(UserRemovedEvent notification, CancellationToken cancellationToken)
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
