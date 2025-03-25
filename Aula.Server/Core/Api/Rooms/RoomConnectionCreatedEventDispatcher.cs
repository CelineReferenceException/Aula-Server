using Aula.Server.Common.Gateway;
using Aula.Server.Domain.Rooms;
using MediatR;

namespace Aula.Server.Core.Api.Rooms;

internal sealed class RoomConnectionCreatedEventDispatcher : INotificationHandler<RoomConnectionCreatedEvent>
{
	private readonly GatewayService _gatewayService;

	public RoomConnectionCreatedEventDispatcher(GatewayService gatewayService)
	{
		_gatewayService = gatewayService;
	}

	public Task Handle(RoomConnectionCreatedEvent notification, CancellationToken cancellationToken)
	{
		var roomConnection = notification.Connection;
		var payload = new GatewayPayload<RoomConnectionEventData>
		{
			Operation = OperationType.Dispatch,
			Event = EventType.RoomConnectionCreated,
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
