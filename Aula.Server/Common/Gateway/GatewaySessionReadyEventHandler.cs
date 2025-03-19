using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

namespace Aula.Server.Common.Gateway;

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

		var payload = new GatewayPayload<HelloEventPayloadData>
		{
			Operation = OperationType.Hello,
			Data = new HelloEventPayloadData
			{
				SessionId = session.Id,
			},
		}.GetJsonUtf8Bytes(_jsonSerializerOptions);

		_ = session.QueueEventAsync(payload, cancellationToken);
		return Task.CompletedTask;
	}
}
