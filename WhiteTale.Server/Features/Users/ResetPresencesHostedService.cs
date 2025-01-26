namespace WhiteTale.Server.Features.Users;

internal sealed class ResetPresencesHostedService : IHostedService, IDisposable
{
	private readonly IServiceScope _serviceScope;

	public ResetPresencesHostedService(IServiceProvider serviceProvider)
	{
		_serviceScope = serviceProvider.CreateScope();
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
		var dbContext = _serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		await foreach (var user in dbContext.Users)
		{
			// We avoid using User.Modify to prevent triggering any events.
			user.Presence = Presence.Offline;
		}

		_ = await dbContext.SaveChangesAsync(cancellationToken);
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(Boolean disposing)
	{
		if (disposing)
		{
			_serviceScope.Dispose();
		}
	}

	~ResetPresencesHostedService()
	{
		Dispose(false);
	}
}
