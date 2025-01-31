using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

namespace WhiteTale.Server.Features.Users.Gateway;

internal sealed class UserUpdatedEventHandler : INotificationHandler<UserUpdatedEvent>
{
	private readonly JsonSerializerOptions _jsonSerializerOptions;
	private readonly GatewayService _gatewayService;

	public UserUpdatedEventHandler(IOptions<JsonOptions> jsonOptions, GatewayService gatewayService)
	{
		_gatewayService = gatewayService;
		_jsonSerializerOptions = jsonOptions.Value.SerializerOptions;
	}

	public async Task Handle(UserUpdatedEvent notification, CancellationToken cancellationToken)
	{
		var user = notification.User;
		var payload = new GatewayPayload<UserData>
		{
			Operation = OperationType.Dispatch,
			Event = EventType.UserUpdated,
			Data = new UserData
			{
				Id = user.Id,
				DisplayName = user.DisplayName,
				Description = user.Description,
				OwnerType = user.OwnerType,
				Presence = user.Presence,
				Permissions = user.Permissions,
				CurrentRoomId = user.CurrentRoomId,
			},
		};
		var jsonString = JsonSerializer.Serialize(payload, _jsonSerializerOptions);
		var payloadBytes = Encoding.UTF8.GetBytes(jsonString);

		foreach (var connection in _gatewayService.Sessions.Values)
		{
			if (!connection.Intents.HasFlag(Intents.Users))
			{
				continue;
			}

			_ = connection.QueueEventAsync(payloadBytes, cancellationToken);
		}
	}
}
