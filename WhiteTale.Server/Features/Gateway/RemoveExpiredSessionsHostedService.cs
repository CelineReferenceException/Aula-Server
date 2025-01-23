namespace WhiteTale.Server.Features.Gateway;

internal sealed class RemoveExpiredSessionsHostedService : BackgroundService
{
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
			ConnectToGateway.RemoveExpiredSessions();
		}
	}
}
