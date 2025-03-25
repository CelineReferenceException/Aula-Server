using System.Text.Json;
using Aula.Server.Common.Gateway;
using Aula.Server.Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

namespace Aula.Server.Core.Api.Users;

internal sealed class UserUpdatedEventDispatcher : INotificationHandler<UserUpdatedEvent>
{
	private readonly GatewayService _gatewayService;
	private readonly JsonSerializerOptions _jsonSerializerOptions;

	public UserUpdatedEventDispatcher(IOptions<JsonOptions> jsonOptions, GatewayService gatewayService)
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
				Type = user.Type,
				Presence = user.Presence,
				Permissions = user.Permissions,
				CurrentRoomId = user.CurrentRoomId,
			},
		};

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
