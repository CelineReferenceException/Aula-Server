using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using WhiteTale.Server.Features.Bans;

namespace WhiteTale.Server.Features.Gateway.Events.Bans;

internal sealed class BanRemovedEventHandler : INotificationHandler<BanRemovedEvent>
{
	private readonly JsonSerializerOptions _jsonSerializerOptions;

	public BanRemovedEventHandler(IOptions<JsonOptions> jsonOptions)
	{
		_jsonSerializerOptions = jsonOptions.Value.SerializerOptions;
	}

	public Task Handle(BanRemovedEvent notification, CancellationToken cancellationToken)
	{
		var ban = notification.Ban;
		var payload = new GatewayPayload<BanData>
		{
			Operation = OperationType.Dispatch,
			Event = EventType.BanRemoved,
			Data = new BanData
			{
				Type = ban.Type,
				ExecutorId = ban.ExecutorId,
				Reason = ban.Reason,
				TargetId = ban.TargetId,
				IpAddress = ban.IpAddress,
				CreationTime = ban.CreationTime,
			},
		}.GetJsonUtf8Bytes(_jsonSerializerOptions);

		foreach (var session in ConnectToGateway.Sessions.Values)
		{
			if (!session.Intents.HasFlag(Intents.Moderation))
			{
				continue;
			}

			_ = session.QueueEventAsync(payload, cancellationToken);
		}

		return Task.CompletedTask;
	}
}
