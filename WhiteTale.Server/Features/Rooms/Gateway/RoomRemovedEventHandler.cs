using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

namespace WhiteTale.Server.Features.Rooms.Gateway;

internal sealed class RoomRemovedEventHandler : INotificationHandler<RoomRemovedEvent>
{
	private readonly JsonSerializerOptions _jsonSerializerOptions;
	private readonly GatewayService _gatewayService;

	public RoomRemovedEventHandler(IOptions<JsonOptions> jsonOptions, GatewayService gatewayService)
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
				CreationTime = room.CreationTime,
			},
		}.GetJsonUtf8Bytes(_jsonSerializerOptions);

		foreach (var connection in _gatewayService.Sessions.Values)
		{
			if (!connection.Intents.HasFlag(Intents.Rooms))
			{
				continue;
			}

			_ = connection.QueueEventAsync(payload, cancellationToken);
		}

		return Task.CompletedTask;
	}
}
