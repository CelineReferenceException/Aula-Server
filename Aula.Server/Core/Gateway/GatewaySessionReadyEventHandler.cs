using System.Text.Json;
using Aula.Server.Core.JsonSerialization;
using MediatR;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

namespace Aula.Server.Core.Gateway;

internal sealed class GatewaySessionReadyEventHandler : INotificationHandler<GatewaySessionReadyEvent>
{
	private readonly JsonSerializerOptions _jsonSerializerOptions;

	public GatewaySessionReadyEventHandler(IOptions<JsonOptions> jsonOptions)
	{
		_jsonSerializerOptions = jsonOptions.Value.SerializerOptions;
	}

	public Task Handle(GatewaySessionReadyEvent notification, CancellationToken cancellationToken)
	{
		var session = notification.Session;

		var payload = new GatewayPayload<GatewaySessionReadyEventPayloadData>
		{
			Operation = OperationType.Dispatch,
			Event = EventType.Ready,
			Data = new GatewaySessionReadyEventPayloadData
			{
				SessionId = session.Id,
			},
		}.GetJsonUtf8Bytes(_jsonSerializerOptions);

		_ = session.QueueEventAsync(payload, cancellationToken);
		return Task.CompletedTask;
	}
}
