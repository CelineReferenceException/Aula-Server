using MediatR;
using Microsoft.EntityFrameworkCore;
using Polly;

namespace WhiteTale.Server.Features.Bans;

internal sealed class BannedUserRestrictor : INotificationHandler<BanCreatedEvent>
{
	private readonly ApplicationDbContext _dbContext;
	private readonly ResiliencePipeline _resiliencePipeline;

	public BannedUserRestrictor(ApplicationDbContext dbContext, IServiceProvider serviceProvider)
	{
		_dbContext = dbContext;
		_resiliencePipeline = serviceProvider.GetRequiredKeyedService<ResiliencePipeline>(BanResiliencePipelineNames.CleanUser);
	}

	public async Task Handle(BanCreatedEvent notification, CancellationToken cancellationToken)
	{
		if (notification.Ban.Type is not BanType.Id)
		{
			return;
		}

		await _resiliencePipeline.ExecuteAsync(async (ban, ct) =>
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

			_ = await _dbContext.SaveChangesAsync(ct);
		}, notification.Ban, cancellationToken);
	}
}
