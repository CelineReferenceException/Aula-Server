using System.Net.WebSockets;
using Aula.Server.Common.Gateway;
using Aula.Server.Common.Persistence;
using Aula.Server.Common.Resilience;
using Aula.Server.Domain.Bans;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Polly;

namespace Aula.Server.Features.Bans;

internal sealed class BannedUserRestrictor : INotificationHandler<BanCreatedEvent>
{
	private readonly ApplicationDbContext _dbContext;
	private readonly GatewayService _gatewayService;
	private readonly ResiliencePipeline _retryOnDbConcurrencyProblem;

	public BannedUserRestrictor(
		ApplicationDbContext dbContext,
		[FromKeyedServices(ResiliencePipelineNames.RetryOnDbConcurrencyProblem)]
		ResiliencePipeline retryOnDbConcurrencyProblem,
		GatewayService gatewayService)
	{
		_dbContext = dbContext;
		_retryOnDbConcurrencyProblem = retryOnDbConcurrencyProblem;
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

		await _retryOnDbConcurrencyProblem.ExecuteAsync(async ct =>
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
			user.SetCurrentRoom(null);
			user.UpdateConcurrencyStamp();

			_ = await _dbContext.SaveChangesAsync(ct);
		}, cancellationToken);
	}
}
