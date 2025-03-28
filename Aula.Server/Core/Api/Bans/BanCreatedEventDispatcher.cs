﻿using System.Diagnostics;
using Aula.Server.Common.Persistence;
using Aula.Server.Domain.Bans;
using Aula.Server.Domain.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Core.Api.Bans;

internal sealed class BanCreatedEventDispatcher : INotificationHandler<BanCreatedEvent>
{
	private readonly ApplicationDbContext _dbContext;
	private readonly GatewaySessionManager _gatewaySessionManager;

	public BanCreatedEventDispatcher(GatewaySessionManager gatewaySessionManager, ApplicationDbContext dbContext)
	{
		_gatewaySessionManager = gatewaySessionManager;
		_dbContext = dbContext;
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
		};

		foreach (var session in _gatewaySessionManager.Sessions.Values)
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
