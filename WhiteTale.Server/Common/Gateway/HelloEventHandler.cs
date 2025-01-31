using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

namespace WhiteTale.Server.Common.Gateway;

internal sealed class HelloEventHandler : INotificationHandler<HelloEvent>
{
	private readonly JsonSerializerOptions _jsonSerializerOptions;

	public HelloEventHandler(IOptions<JsonOptions> jsonOptions)
	{
		_jsonSerializerOptions = jsonOptions.Value.SerializerOptions;
	}

	public Task Handle(HelloEvent notification, CancellationToken cancellationToken)
	{
		var session = notification.Session;

		var payload = new GatewayPayload<HelloEventPayloadData>
		{
			Operation = OperationType.Hello,
			Data = new HelloEventPayloadData
			{
				SessionId = session.Id,
			},
		};
		var payloadBytes = JsonSerializer.SerializeToUtf8Bytes(payload, _jsonSerializerOptions);

		_ = session.QueueEventAsync(payloadBytes, cancellationToken);
		return Task.CompletedTask;
	}
}
