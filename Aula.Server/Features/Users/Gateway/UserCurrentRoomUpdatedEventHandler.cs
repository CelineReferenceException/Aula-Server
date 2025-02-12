using System.Text.Json;
using Aula.Server.Common.Gateway;
using Aula.Server.Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

namespace Aula.Server.Features.Users.Gateway;

internal sealed class UserCurrentRoomUpdatedEventHandler : INotificationHandler<UserCurrentRoomUpdatedEvent>
{
	private readonly GatewayService _gatewayService;
	private readonly JsonSerializerOptions _jsonSerializerOptions;

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
