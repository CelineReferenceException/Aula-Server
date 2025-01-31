using MediatR;
using Microsoft.EntityFrameworkCore;

namespace WhiteTale.Server.Features.Bans;

internal sealed class BannedUserRestrictor : INotificationHandler<BanCreatedEvent>
{
	private readonly ApplicationDbContext _dbContext;

	public BannedUserRestrictor(ApplicationDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	public async Task Handle(BanCreatedEvent notification, CancellationToken cancellationToken)
	{
		var ban = notification.Ban;
		if (ban.Type is not BanType.Id)
		{
			return;
		}

		var user = await _dbContext.Users
			.AsTracking()
			.Where(u => u.Id == ban.TargetId)
			.FirstOrDefaultAsync(cancellationToken);
		if (user is null)
		{
			return;
		}

		user.Modify(permissions: 0);
		user.UpdateConcurrencyStamp();

		_ = await _dbContext.SaveChangesAsync(cancellationToken);
	}
}
