using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

namespace WhiteTale.Server.Features.RoomConnections.Gateway;

internal sealed class RoomConnectionCreatedEventHandler : INotificationHandler<RoomConnectionCreatedEvent>
{
	private readonly JsonSerializerOptions _jsonSerializerOptions;
	private readonly GatewayService _gatewayService;

	public RoomConnectionCreatedEventHandler(IOptions<JsonOptions> jsonOptions, GatewayService gatewayService)
	{
		_gatewayService = gatewayService;
		_jsonSerializerOptions = jsonOptions.Value.SerializerOptions;
	}

	public async Task Handle(RoomConnectionCreatedEvent notification, CancellationToken cancellationToken)
	{
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

		foreach (var gatewayConnection in _gatewayService.Sessions.Values)
		{
			if (!gatewayConnection.Intents.HasFlag(Intents.Rooms))
			{
				continue;
			}

			_ = gatewayConnection.QueueEventAsync(payloadBytes, cancellationToken);
		}
	}
}
