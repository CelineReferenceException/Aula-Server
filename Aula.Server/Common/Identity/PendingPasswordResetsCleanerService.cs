namespace Aula.Server.Common.Identity;

internal sealed class PendingPasswordResetsCleanerService : BackgroundService
{
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			UserManager.CleanPendingPasswordResets();
			await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
		}
	}
}
