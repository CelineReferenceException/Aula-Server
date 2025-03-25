using Aula.Server.Domain.Users;
using MediatR;

namespace Aula.Server.Core.Api.Users;

internal sealed class UserCurrentRoomUpdatedEventDispatcher : INotificationHandler<UserCurrentRoomUpdatedEvent>
{
	private readonly GatewayService _gatewayService;

	public UserCurrentRoomUpdatedEventDispatcher(GatewayService gatewayService)
	{
		_gatewayService = gatewayService;
	}

	public Task Handle(UserCurrentRoomUpdatedEvent notification, CancellationToken cancellationToken)
	{
		var payload = new GatewayPayload<UserCurrentRoomUpdatedEventData>
		{
			Operation = OperationType.Dispatch,
			Event = EventType.UserCurrentRoomUpdated,
			Data = new UserCurrentRoomUpdatedEventData
			{
				UserId = notification.UserId,
				PreviousRoomId = notification.PreviousRoomId,
				CurrentRoomId = notification.CurrentRoomId,
			},
		};

		foreach (var session in _gatewayService.Sessions.Values)
		{
			if (!session.Intents.HasFlag(Intents.Users))
			{
				continue;
			}

			_ = session.QueueEventAsync(payload, cancellationToken);
		}

		return Task.CompletedTask;
	}
}
