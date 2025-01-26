namespace WhiteTale.Server.Features.Users;

internal sealed class ResetPresencesHostedService : IHostedService
{
	private readonly ApplicationDbContext _dbContext;

	public ResetPresencesHostedService(ApplicationDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		await ResetPresencesAsync(cancellationToken);
	}

	public async Task StopAsync(CancellationToken cancellationToken)
	{
		await ResetPresencesAsync(cancellationToken);
	}

	private async Task ResetPresencesAsync(CancellationToken cancellationToken)
	{
		await foreach (var user in _dbContext.Users)
		{
			// We avoid using User.Modify to prevent triggering any events.
			user.Presence = Presence.Offline;
		}

		_ = await _dbContext.SaveChangesAsync(cancellationToken);
	}
}
