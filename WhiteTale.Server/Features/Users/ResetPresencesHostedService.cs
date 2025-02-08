using Microsoft.EntityFrameworkCore;

namespace WhiteTale.Server.Features.Users;

internal sealed class ResetPresencesHostedService : IHostedService, IDisposable
{
	private readonly IServiceScope _serviceScope;

	public ResetPresencesHostedService(IServiceProvider serviceProvider)
	{
		_serviceScope = serviceProvider.CreateScope();
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
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
		_ = await dbContext.Users
			.ExecuteUpdateAsync(setPropertyCalls => setPropertyCalls
				.SetProperty(property => property.Presence, value => Presence.Offline), cancellationToken);
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
