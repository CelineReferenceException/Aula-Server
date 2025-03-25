using Aula.Server.Domain.Rooms;
using MediatR;

namespace Aula.Server.Core.Api.Rooms;

internal sealed class RoomRemovedEventDispatcher : INotificationHandler<RoomRemovedEvent>
{
	private readonly GatewayService _gatewayService;

	public RoomRemovedEventDispatcher(GatewayService gatewayService)
	{
		_gatewayService = gatewayService;
	}

	public Task Handle(RoomRemovedEvent notification, CancellationToken cancellationToken)
	{
		var room = notification.Room;
		var payload = new GatewayPayload<RoomData>
		{
			Operation = OperationType.Dispatch,
			Event = EventType.RoomRemoved,
			Data = new RoomData
			{
				Id = room.Id,
				Name = room.Name,
				Description = room.Description,
				IsEntrance = room.IsEntrance,
				ConnectedRoomIds = room.Connections.Select(x => x.TargetRoomId).ToArray(),
				CreationDate = room.CreationDate,
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
