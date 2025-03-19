namespace Aula.Server.Common.Gateway;

internal sealed class ExpiredSessionsCleanerService : BackgroundService
{
	private readonly GatewayService _gatewayService;

	public ExpiredSessionsCleanerService(GatewayService gatewayService)
	{
		_gatewayService = gatewayService;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			await Task.Delay(_gatewayService.ExpirePeriod, stoppingToken);
			_gatewayService.ClearExpiredSessions();
		}
	}
}
