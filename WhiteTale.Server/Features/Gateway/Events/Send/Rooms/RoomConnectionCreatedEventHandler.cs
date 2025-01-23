using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

namespace WhiteTale.Server.Features.Gateway.Events.Send.Rooms;

internal sealed class RoomConnectionCreatedEventHandler : INotificationHandler<RoomConnectionCreatedEvent>
{
	private readonly JsonSerializerOptions _jsonSerializerOptions;

	public RoomConnectionCreatedEventHandler(IOptions<JsonOptions> jsonOptions)
	{
		_jsonSerializerOptions = jsonOptions.Value.SerializerOptions;
	}

	public async Task Handle(RoomConnectionCreatedEvent notification, CancellationToken cancellationToken)
	{
		var operations = new List<Task>();

		var roomConnection = notification.Connection;
		var payload = new GatewayPayload<RoomConnectionData>
		{
			Operation = OperationType.Dispatch,
			Event = EventType.RoomConnectionCreated,
			Data = new RoomConnectionData
			{
				SourceRoomId = roomConnection.SourceRoomId,
				TargetRoomId = roomConnection.TargetRoomId,
			},
		};
		var payloadBytes = JsonSerializer.SerializeToUtf8Bytes(payload, _jsonSerializerOptions);

		foreach (var gatewayConnection in ConnectToGateway.Sessions.Values)
		{
			if (!gatewayConnection.Intents.HasFlag(Intents.Rooms))
			{
				continue;
			}

			var operation = gatewayConnection.QueueEventAsync(payloadBytes, cancellationToken);
			operations.Add(operation);
		}

		await Task.WhenAll(operations);
	}
}
