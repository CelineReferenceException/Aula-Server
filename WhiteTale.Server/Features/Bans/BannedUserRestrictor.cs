using MediatR;
using Microsoft.EntityFrameworkCore;

namespace WhiteTale.Server.Features.Bans;

internal sealed class BannedUserRestrictor : INotificationHandler<BanCreatedEvent>
{
	private readonly ApplicationDbContext _dbContext;
	private readonly ResiliencePipelines _resiliencePipelines;

	public BannedUserRestrictor(ApplicationDbContext dbContext, ResiliencePipelines resiliencePipelines)
	{
		_dbContext = dbContext;
		_resiliencePipelines = resiliencePipelines;
	}

	public async Task Handle(BanCreatedEvent notification, CancellationToken cancellationToken)
	{
		var ban = notification.Ban;
		if (ban.Type is not BanType.Id)
		{
			return;
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
