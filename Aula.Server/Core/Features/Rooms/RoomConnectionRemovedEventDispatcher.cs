﻿using System.Text.Json;
using Aula.Server.Core.Gateway;
using Aula.Server.Core.Json;
using MediatR;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

namespace Aula.Server.Core.Features.Rooms;

internal sealed class RoomConnectionRemovedEventDispatcher : INotificationHandler<RoomConnectionRemovedEvent>
{
	private readonly GatewayService _gatewayService;
	private readonly JsonSerializerOptions _jsonSerializerOptions;

	public RoomConnectionRemovedEventDispatcher(IOptions<JsonOptions> jsonOptions, GatewayService gatewayService)
	{
		_gatewayService = gatewayService;
		_jsonSerializerOptions = jsonOptions.Value.SerializerOptions;
	}

	public Task Handle(RoomConnectionRemovedEvent notification, CancellationToken cancellationToken)
	{
		var roomConnection = notification.Connection;
		var payload = new GatewayPayload<RoomConnectionEventData>
		{
			Operation = OperationType.Dispatch,
			Event = EventType.RoomConnectionRemoved,
			Data = new RoomConnectionEventData
			{
				SourceRoomId = roomConnection.SourceRoomId,
				TargetRoomId = roomConnection.TargetRoomId,
			},
		}.GetJsonUtf8Bytes(_jsonSerializerOptions);

		foreach (var session in _gatewayService.Sessions.Values)
		{
			if (!session.Intents.HasFlag(Intents.Rooms))
			{
				continue;
			}

			_ = session.QueueEventAsync(payload, cancellationToken);
		}

		return Task.CompletedTask;
	}
}
