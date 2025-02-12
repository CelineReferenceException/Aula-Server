using System.Net.WebSockets;
using Aula.Server.Common.Gateway;
using Aula.Server.Domain.Users;
using MediatR;

namespace Aula.Server.Features.Identity.Gateway;

internal sealed class UserSecurityStampUpdatedEventHandler : INotificationHandler<UserSecurityStampUpdatedEvent>
{
	private readonly GatewayService _gatewayService;

	public UserSecurityStampUpdatedEventHandler(GatewayService gatewayService)
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
