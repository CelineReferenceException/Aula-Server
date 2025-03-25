namespace Aula.Server.Common.Identity;

internal sealed class PendingEmailConfirmationsCleanerService : BackgroundService
{
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			UserManager.CleanPendingEmailConfirmations();
			await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
		}
	}
}
