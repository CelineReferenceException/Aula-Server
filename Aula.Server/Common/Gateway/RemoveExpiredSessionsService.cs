namespace Aula.Server.Common.Gateway;

internal sealed class RemoveExpiredSessionsService : BackgroundService
{
	private readonly GatewayService _gatewayService;

	public RemoveExpiredSessionsService(GatewayService gatewayService)
	{
		_gatewayService = gatewayService;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			await Task.Delay(_gatewayService.TimeToExpire, stoppingToken);
			_gatewayService.RemoveExpiredSessions();
		}
	}
}
