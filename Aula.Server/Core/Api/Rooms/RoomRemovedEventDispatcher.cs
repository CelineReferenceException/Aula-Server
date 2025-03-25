using System.Text.Json;
using Aula.Server.Common.Gateway;
using Aula.Server.Domain.Rooms;
using MediatR;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

namespace Aula.Server.Core.Api.Rooms;

internal sealed class RoomRemovedEventDispatcher : INotificationHandler<RoomRemovedEvent>
{
	private readonly GatewayService _gatewayService;
	private readonly JsonSerializerOptions _jsonSerializerOptions;

	public RoomRemovedEventDispatcher(IOptions<JsonOptions> jsonOptions, GatewayService gatewayService)
	{
		_gatewayService = gatewayService;
		_jsonSerializerOptions = jsonOptions.Value.SerializerOptions;
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
