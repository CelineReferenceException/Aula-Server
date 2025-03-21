using System.Diagnostics;
using System.Text.Json;
using Aula.Server.Core.Gateway;
using Aula.Server.Core.JsonSerialization;
using Aula.Server.Core.Persistence;
using MediatR;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Aula.Server.Core.Features.Bans.Gateway;

internal sealed class BanCreatedEventDispatcher : INotificationHandler<BanCreatedEvent>
{
	private readonly ApplicationDbContext _dbContext;
	private readonly GatewayService _gatewayService;
	private readonly JsonSerializerOptions _jsonSerializerOptions;

	public BanCreatedEventDispatcher(IOptions<JsonOptions> jsonOptions, GatewayService gatewayService, ApplicationDbContext dbContext)
	{
		_gatewayService = gatewayService;
		_dbContext = dbContext;
		_jsonSerializerOptions = jsonOptions.Value.SerializerOptions;
	}

	public async Task Handle(BanCreatedEvent notification, CancellationToken cancellationToken)
	{
		var ban = notification.Ban;
		var payload = new GatewayPayload<BanData>
		{
			Operation = OperationType.Dispatch,
			Event = EventType.BanCreated,
			Data = new BanData
			{
				Type = ban.Type,
				ExecutorId = ban.ExecutorId,
				Reason = ban.Reason,
				TargetId = ban.TargetId,
				CreationDate = ban.CreationDate,
			},
		}.GetJsonUtf8Bytes(_jsonSerializerOptions);

		foreach (var session in _gatewayService.Sessions.Values)
		{
			var sessionUser = await _dbContext.Users
				.Where(u => u.Id == session.UserId)
				.Select(u => new
				{
					u.Permissions,
				})
				.FirstOrDefaultAsync(cancellationToken) ?? throw new UnreachableException("User should exist");

			if (!session.Intents.HasFlag(Intents.Moderation) &&
			    (sessionUser.Permissions.HasFlag(Permissions.BanUsers) ||
			     sessionUser.Permissions.HasFlag(Permissions.Administrator)))
			{
				continue;
			}

			_ = session.QueueEventAsync(payload, cancellationToken);
		}
	}
}
