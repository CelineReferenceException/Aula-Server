using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using WhiteTale.Server.Features.Users;

namespace WhiteTale.Server.Features.Gateway.Events.Users;

internal sealed class UserUpdatedEventHandler : INotificationHandler<UserUpdatedEvent>
{
	private readonly JsonSerializerOptions _jsonSerializerOptions;

	public UserUpdatedEventHandler(IOptions<JsonOptions> jsonOptions)
	{
		_jsonSerializerOptions = jsonOptions.Value.SerializerOptions;
	}

	public async Task Handle(UserUpdatedEvent notification, CancellationToken cancellationToken)
	{
		var operations = new List<Task>();

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
