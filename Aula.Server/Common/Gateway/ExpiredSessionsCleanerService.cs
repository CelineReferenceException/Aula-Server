namespace Aula.Server.Common.Gateway;

internal sealed class ExpiredSessionsCleanerService : BackgroundService
{
	private readonly GatewayService _gatewayService;
	private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(5);

	public ExpiredSessionsCleanerService(GatewayService gatewayService)
	{
		_gatewayService = gatewayService;
		_cleanupInterval = gatewayService.ExpirePeriod < _cleanupInterval ? gatewayService.ExpirePeriod : _cleanupInterval;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			await Task.Delay(_cleanupInterval, stoppingToken);
			_gatewayService.ClearExpiredSessions();
		}
	}
}
