using System.Net.WebSockets;
using MediatR;

namespace WhiteTale.Server.Features.Identity.Gateway;

internal sealed class UserRemovedEventHandler : INotificationHandler<UserRemovedEvent>
{
	private readonly GatewayService _gatewayService;

	public UserRemovedEventHandler(GatewayService gatewayService)
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
