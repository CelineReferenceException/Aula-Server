using Aula.Server.Common.Persistence;
using Aula.Server.Domain.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Core.Api.Bans;

internal sealed class UserRemovedBanRemover : INotificationHandler<UserRemovedEvent>
{
	private readonly ApplicationDbContext _dbContext;

	public UserRemovedBanRemover(ApplicationDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	public async Task Handle(UserRemovedEvent notification, CancellationToken cancellationToken)
	{
		var ban = await _dbContext.Bans
			.Where(b => b.TargetId == notification.User.Id)
			.FirstOrDefaultAsync(cancellationToken);
		if (ban is null)
		{
			return;
		}

		ban.Remove();
		_ = _dbContext.Remove(ban);
		_ = await _dbContext.SaveChangesAsync(cancellationToken);
	}
}
