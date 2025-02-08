using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

namespace WhiteTale.Server.Features.Users.Gateway;

internal sealed class UserUpdatedEventHandler : INotificationHandler<UserUpdatedEvent>
{
	private readonly GatewayService _gatewayService;
	private readonly JsonSerializerOptions _jsonSerializerOptions;

	public UserUpdatedEventHandler(IOptions<JsonOptions> jsonOptions, GatewayService gatewayService)
	{
		_gatewayService = gatewayService;
		_jsonSerializerOptions = jsonOptions.Value.SerializerOptions;
	}

	public Task Handle(UserUpdatedEvent notification, CancellationToken cancellationToken)
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
		}.GetJsonUtf8Bytes(_jsonSerializerOptions);

		foreach (var session in _gatewayService.Sessions.Values)
		{
			if (!session.Intents.HasFlag(Intents.Users))
			{
				continue;
			}

			_ = session.QueueEventAsync(payload, cancellationToken);
		}

		return Task.CompletedTask;
	}
}
