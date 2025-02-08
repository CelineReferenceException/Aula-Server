using System.Net.WebSockets;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace WhiteTale.Server.Features.Bans;

internal sealed class BannedUserRestrictor : INotificationHandler<BanCreatedEvent>
{
	private readonly ApplicationDbContext _dbContext;
	private readonly ResiliencePipelines _resiliencePipelines;
	private readonly GatewayService _gatewayService;

	public BannedUserRestrictor(ApplicationDbContext dbContext, ResiliencePipelines resiliencePipelines, GatewayService gatewayService)
	{
		_dbContext = dbContext;
		_resiliencePipelines = resiliencePipelines;
		_gatewayService = gatewayService;
	}

	public async Task Handle(BanCreatedEvent notification, CancellationToken cancellationToken)
	{
		var ban = notification.Ban;
		if (ban.Type is not BanType.Id)
		{
			return;
		}

		var targetSessions = _gatewayService.Sessions
			.Select(kvp => kvp.Value)
			.Where(s => s.UserId == ban.TargetId);
		foreach (var session in targetSessions)
		{
			await session.StopAsync(WebSocketCloseStatus.Empty);
		}

		await _resiliencePipelines.RetryOnDbConcurrencyProblem.ExecuteAsync(async ct =>
		{
			var user = await _dbContext.Users
				.AsTracking()
				.Where(u => u.Id == ban.TargetId)
				.FirstOrDefaultAsync(ct);
			if (user is null)
			{
				return;
			}

			user.Modify(permissions: 0);
			user.UpdateConcurrencyStamp();

			_ = await _dbContext.SaveChangesAsync(ct);
		}, cancellationToken);
	}
}
