using Microsoft.Extensions.Options;

namespace Aula.Server.Common.RateLimiting;

internal sealed partial class ClearRateLimitersOnConfigurationUpdateService : IHostedService
{
	private readonly ILogger<ClearRateLimitersOnConfigurationUpdateService> _logger;
	private readonly IOptionsMonitor<RateLimitOptions> _optionsMonitor;
	private readonly RateLimiterManager _rateLimiterManager;
	private DateTime _lastClearDate = DateTime.UtcNow;
	private IDisposable? _listenerDisposable;

	public ClearRateLimitersOnConfigurationUpdateService(
		RateLimiterManager rateLimiterManager,
		IOptionsMonitor<RateLimitOptions> optionsMonitor,
		ILogger<ClearRateLimitersOnConfigurationUpdateService> logger)
	{
		_rateLimiterManager = rateLimiterManager;
		_optionsMonitor = optionsMonitor;
		_logger = logger;
	}

	public Task StartAsync(CancellationToken cancellationToken)
	{
		_listenerDisposable = _optionsMonitor.OnChange(_ =>
		{
			// The file watcher sometimes trigger more than once for the same change.
			if (DateTime.UtcNow - _lastClearDate < TimeSpan.FromSeconds(1))
			{
				return;
			}

			_lastClearDate = DateTime.UtcNow;
			var count = _rateLimiterManager.ClearRateLimiters();
			LogRateLimitersClear(_logger, count);
		});
		return Task.CompletedTask;
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		_listenerDisposable?.Dispose();
		return Task.CompletedTask;
	}

	[LoggerMessage(LogLevel.Information,
		Message = "Rate Limiting configuration has been updated. ${count} rate limiters have been cleared.")]
	private static partial void LogRateLimitersClear(ILogger logger, Int32 count);
}
