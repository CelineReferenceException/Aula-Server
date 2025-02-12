using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

namespace Aula.Server.Common.Gateway;

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
		}.GetJsonUtf8Bytes(_jsonSerializerOptions);

		_ = session.QueueEventAsync(payload, cancellationToken);
		return Task.CompletedTask;
	}
}
