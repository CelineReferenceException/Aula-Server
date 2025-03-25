using System.Text.Json;
using Aula.Server.Core.Domain.Users;
using Aula.Server.Core.Gateway;
using Aula.Server.Core.Json;
using MediatR;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

namespace Aula.Server.Core.Features.Users;

internal sealed class UserCurrentRoomUpdatedEventDispatcher : INotificationHandler<UserCurrentRoomUpdatedEvent>
{
	private readonly GatewayService _gatewayService;
	private readonly JsonSerializerOptions _jsonSerializerOptions;

	public UserCurrentRoomUpdatedEventDispatcher(IOptions<JsonOptions> jsonOptions, GatewayService gatewayService)
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
