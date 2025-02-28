using Microsoft.Extensions.Options;

namespace Aula.Server.Common.RateLimiting;

internal sealed partial class ClearCacheOnConfigurationUpdateService : IHostedService
{
	private readonly RateLimiterManager _rateLimiterManager;
	private readonly IOptionsMonitor<RateLimitOptions> _optionsMonitor;
	private readonly ILogger<ClearCacheOnConfigurationUpdateService> _logger;
	private IDisposable? _listenerDisposable;
	private DateTime _lastClearDateTime = DateTime.UtcNow;

	public ClearCacheOnConfigurationUpdateService(
		RateLimiterManager rateLimiterManager,
		IOptionsMonitor<RateLimitOptions> optionsMonitor,
		ILogger<ClearCacheOnConfigurationUpdateService> logger)
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
			if (DateTime.UtcNow - _lastClearDateTime < TimeSpan.FromSeconds(1))
			{
				return;
			}

			_lastClearDateTime = DateTime.UtcNow;
			var count = _rateLimiterManager.ClearCache();
			LogCacheClear(_logger, count);
		});
		return Task.CompletedTask;
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		_listenerDisposable?.Dispose();
		return Task.CompletedTask;
	}

	[LoggerMessage(LogLevel.Information, Message = "Rate limiters cache clear, {count} elements removed.")]
	private static partial void LogCacheClear(ILogger logger, Int32 count);
}
