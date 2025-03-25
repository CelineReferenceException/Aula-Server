using Aula.Server.Domain.Users;
using MediatR;

namespace Aula.Server.Core.Api.Users;

internal sealed class UserCurrentRoomUpdatedEventDispatcher : INotificationHandler<UserCurrentRoomUpdatedEvent>
{
	private readonly GatewaySessionManager _gatewaySessionManager;

	public UserCurrentRoomUpdatedEventDispatcher(GatewaySessionManager gatewaySessionManager)
	{
		_gatewaySessionManager = gatewaySessionManager;
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

		foreach (var session in _gatewaySessionManager.Sessions.Values)
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
