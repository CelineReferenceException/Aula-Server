using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

namespace WhiteTale.Server.Features.Users.Gateway;

internal sealed class UserCurrentRoomUpdatedEventHandler : INotificationHandler<UserCurrentRoomUpdatedEvent>
{
	private readonly JsonSerializerOptions _jsonSerializerOptions;
	private readonly GatewayService _gatewayService;

	public UserCurrentRoomUpdatedEventHandler(IOptions<JsonOptions> jsonOptions, GatewayService gatewayService)
	{
		_gatewayService = gatewayService;
		_jsonSerializerOptions = jsonOptions.Value.SerializerOptions;
	}

	public Task Handle(UserCurrentRoomUpdatedEvent notification, CancellationToken cancellationToken)
	{
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
		}.GetJsonUtf8Bytes(_jsonSerializerOptions);

		foreach (var connection in _gatewayService.Sessions.Values)
		{
			if (!connection.Intents.HasFlag(Intents.Users))
			{
				continue;
			}

			_ = connection.QueueEventAsync(payload, cancellationToken);
		}

		return Task.CompletedTask;
	}
}
