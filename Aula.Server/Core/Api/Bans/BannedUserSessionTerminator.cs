using System.Net.WebSockets;
using Aula.Server.Common.Persistence;
using Aula.Server.Common.Resilience;
using Aula.Server.Domain.Bans;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Polly;

namespace Aula.Server.Core.Api.Bans;

internal sealed class BannedUserSessionTerminator : INotificationHandler<BanCreatedEvent>
{
	private readonly ApplicationDbContext _dbContext;
	private readonly GatewaySessionManager _gatewaySessionManager;
	private readonly ResiliencePipeline _retryOnDbConcurrencyProblem;

	public BannedUserSessionTerminator(
		ApplicationDbContext dbContext,
		[FromKeyedServices(ResiliencePipelines.RetryOnDbConcurrencyProblem)] ResiliencePipeline retryOnDbConcurrencyProblem,
		GatewaySessionManager gatewaySessionManager)
	{
		_dbContext = dbContext;
		_retryOnDbConcurrencyProblem = retryOnDbConcurrencyProblem;
		_gatewaySessionManager = gatewaySessionManager;
	}

	public async Task Handle(BanCreatedEvent notification, CancellationToken cancellationToken)
	{
		var ban = notification.Ban;
		if (ban.Type is not BanType.Id)
		{
			return;
		}

		var targetSessions = _gatewaySessionManager.Sessions
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

			user.Modify(permissions: 0).ThrowIfFailed();
			user.SetCurrentRoom(null);
			user.UpdateConcurrencyStamp();

			_ = await _dbContext.SaveChangesAsync(ct);
		}, cancellationToken);
	}
}
