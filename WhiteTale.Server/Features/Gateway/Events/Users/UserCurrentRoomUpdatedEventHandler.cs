using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

namespace WhiteTale.Server.Features.Gateway.Events.Users;

internal sealed class UserCurrentRoomUpdatedEventHandler : INotificationHandler<UserCurrentRoomUpdatedEvent>
{
	private readonly JsonSerializerOptions _jsonSerializerOptions;

	public UserCurrentRoomUpdatedEventHandler(IOptions<JsonOptions> jsonOptions)
	{
		_jsonSerializerOptions = jsonOptions.Value.SerializerOptions;
	}

	public async Task Handle(UserCurrentRoomUpdatedEvent notification, CancellationToken cancellationToken)
	{
		var operations = new List<Task>();

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
		var payloadBytes = JsonSerializer.SerializeToUtf8Bytes(payload, _jsonSerializerOptions);

		foreach (var connection in ConnectToGateway.Sessions.Values)
		{
			if (!connection.Intents.HasFlag(Intents.Users))
			{
				continue;
			}

			var operation = connection.QueueEventAsync(payloadBytes, cancellationToken);
			operations.Add(operation);
		}

		await Task.WhenAll(operations);
	}
}
