namespace Aula.Server.Common.Gateway;

internal sealed class ExpiredSessionsCleanerService : BackgroundService
{
	private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(5);
	private readonly GatewaySessionManager _gatewaySessionManager;

	public ExpiredSessionsCleanerService(GatewaySessionManager gatewaySessionManager)
	{
		_gatewaySessionManager = gatewaySessionManager;
		_cleanupInterval = gatewaySessionManager.ExpirePeriod < _cleanupInterval ? gatewaySessionManager.ExpirePeriod : _cleanupInterval;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			await Task.Delay(_cleanupInterval, stoppingToken);
			_gatewaySessionManager.ClearExpiredSessions();
		}
	}
}
