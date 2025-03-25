using Aula.Server.Domain.Rooms;
using MediatR;

namespace Aula.Server.Core.Api.Rooms;

internal sealed class RoomConnectionRemovedEventDispatcher : INotificationHandler<RoomConnectionRemovedEvent>
{
	private readonly GatewayService _gatewayService;

	public RoomConnectionRemovedEventDispatcher(GatewayService gatewayService)
	{
		_gatewayService = gatewayService;
	}

	public Task Handle(RoomConnectionRemovedEvent notification, CancellationToken cancellationToken)
	{
		var roomConnection = notification.Connection;
		var payload = new GatewayPayload<RoomConnectionEventData>
		{
			Operation = OperationType.Dispatch,
			Event = EventType.RoomConnectionRemoved,
			Data = new RoomConnectionEventData
			{
				SourceRoomId = roomConnection.SourceRoomId,
				TargetRoomId = roomConnection.TargetRoomId,
			},
		};

		foreach (var session in _gatewayService.Sessions.Values)
		{
			if (!session.Intents.HasFlag(Intents.Rooms))
			{
				continue;
			}

			_ = session.QueueEventAsync(payload, cancellationToken);
		}

		return Task.CompletedTask;
	}
}
