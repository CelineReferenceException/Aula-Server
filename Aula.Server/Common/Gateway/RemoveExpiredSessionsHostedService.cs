namespace Aula.Server.Common.Gateway;

internal sealed class RemoveExpiredSessionsHostedService : BackgroundService
{
	private readonly GatewayService _gatewayService;

	public RemoveExpiredSessionsHostedService(GatewayService gatewayService)
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
