using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

namespace WhiteTale.Server.Features.Rooms.Gateway;

internal sealed class RoomUpdatedEventHandler : INotificationHandler<RoomUpdatedEvent>
{
	private readonly JsonSerializerOptions _jsonSerializerOptions;
	private readonly GatewayService _gatewayService;

	public RoomUpdatedEventHandler(IOptions<JsonOptions> jsonOptions, GatewayService gatewayService)
	{
		_gatewayService = gatewayService;
		_jsonSerializerOptions = jsonOptions.Value.SerializerOptions;
	}

	public async Task Handle(RoomUpdatedEvent notification, CancellationToken cancellationToken)
	{
		var room = notification.Room;
		var payload = new GatewayPayload<RoomData>
		{
			Operation = OperationType.Dispatch,
			Event = EventType.RoomUpdated,
			Data = new RoomData
			{
				Id = room.Id,
				Name = room.Name,
				Description = room.Description,
				IsEntrance = room.IsEntrance,
				CreationTime = room.CreationTime,
			},
		};
		var payloadBytes = JsonSerializer.SerializeToUtf8Bytes(payload, _jsonSerializerOptions);

		foreach (var connection in _gatewayService.Sessions.Values)
		{
			if (!connection.Intents.HasFlag(Intents.Rooms))
			{
				continue;
			}

			_ = connection.QueueEventAsync(payloadBytes, cancellationToken);
		}
	}
}
