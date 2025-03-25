using System.Diagnostics;
using Aula.Server.Common.Gateway;
using Aula.Server.Common.Persistence;
using Aula.Server.Domain.Bans;
using Aula.Server.Domain.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Core.Api.Bans;

internal sealed class BanRemovedEventDispatcher : INotificationHandler<BanRemovedEvent>
{
	private readonly ApplicationDbContext _dbContext;
	private readonly GatewayService _gatewayService;

	public BanRemovedEventDispatcher(GatewayService gatewayService, ApplicationDbContext dbContext)
	{
		_gatewayService = gatewayService;
		_dbContext = dbContext;
	}

	public async Task Handle(BanRemovedEvent notification, CancellationToken cancellationToken)
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
				CreationDate = ban.CreationDate,
			},
		};

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
