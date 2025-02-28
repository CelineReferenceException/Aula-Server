namespace Aula.Server.Common.RateLimiting;

internal sealed class RemoveUnusedRateLimitersService : BackgroundService
{
	private static readonly TimeSpan s_interval = TimeSpan.FromMinutes(1);
	private readonly RateLimiterManager _rateLimiterManager;

	public RemoveUnusedRateLimitersService(RateLimiterManager rateLimiterManager)
	{
		_rateLimiterManager = rateLimiterManager;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			await Task.Delay(s_interval, stoppingToken);
			_rateLimiterManager.RemoveUnusedReplenishingRateLimiters();
		}
	}
}
