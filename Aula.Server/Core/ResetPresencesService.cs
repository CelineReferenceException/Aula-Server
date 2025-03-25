using Aula.Server.Common.Persistence;
using Aula.Server.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Core;

internal sealed class ResetPresencesService : IHostedService, IDisposable
{
	private readonly IServiceScope _serviceScope;

	public ResetPresencesService(IServiceProvider serviceProvider)
	{
		_serviceScope = serviceProvider.CreateScope();
	}

	~ResetPresencesService()
	{
		Dispose(false);
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	public Task StartAsync(CancellationToken cancellationToken)
	{
		return Task.CompletedTask;
	}

	public async Task StopAsync(CancellationToken cancellationToken)
	{
		await ResetPresencesAsync(cancellationToken);
	}

	private async Task ResetPresencesAsync(CancellationToken cancellationToken)
	{
		var dbContext = _serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		_ = await dbContext.Users
			.ExecuteUpdateAsync(setPropertyCalls => setPropertyCalls.SetProperty(property => property.Presence, value => Presence.Offline),
				cancellationToken);
	}

	private void Dispose(Boolean disposing)
	{
		if (disposing)
		{
			_serviceScope.Dispose();
		}
	}
}
