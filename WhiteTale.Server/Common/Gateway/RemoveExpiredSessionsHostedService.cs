namespace WhiteTale.Server.Common.Gateway;

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
			await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
			_gatewayService.RemoveExpiredSessions();
		}
	}
}
