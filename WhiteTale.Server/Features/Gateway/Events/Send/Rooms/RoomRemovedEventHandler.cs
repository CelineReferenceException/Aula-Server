using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using WhiteTale.Server.Features.Rooms;

namespace WhiteTale.Server.Features.Gateway.Events.Send.Rooms;

internal sealed class RoomRemovedEventHandler : INotificationHandler<RoomRemovedEvent>
{
	private readonly JsonSerializerOptions _jsonSerializerOptions;

	public RoomRemovedEventHandler(IOptions<JsonOptions> jsonOptions)
	{
		_jsonSerializerOptions = jsonOptions.Value.SerializerOptions;
	}

	public async Task Handle(RoomRemovedEvent notification, CancellationToken cancellationToken)
	{
		var operations = new List<Task>();

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
		};
		var payloadBytes = JsonSerializer.SerializeToUtf8Bytes(payload, _jsonSerializerOptions);

		foreach (var connection in ConnectToGateway.Sessions.Values)
		{
			if (!connection.Intents.HasFlag(Intents.Rooms))
			{
				continue;
			}

			var operation = connection.QueueEventAsync(payloadBytes, cancellationToken);
			operations.Add(operation);
		}

		await Task.WhenAll(operations);
	}
}
