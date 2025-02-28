using Microsoft.Extensions.Options;

namespace Aula.Server.Common.RateLimiting;

internal sealed class ClearCacheOnConfigurationUpdateService : IHostedService
{
	private readonly RateLimiterManager _rateLimiterManager;
	private readonly IOptionsMonitor<RateLimitOptions> _optionsMonitor;
	private IDisposable? _listenerDisposable;
	private DateTime _lastClearDateTime = DateTime.UtcNow;

	public ClearCacheOnConfigurationUpdateService(
		RateLimiterManager rateLimiterManager,
		IOptionsMonitor<RateLimitOptions> optionsMonitor)
	{
		_rateLimiterManager = rateLimiterManager;
		_optionsMonitor = optionsMonitor;
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
			_rateLimiterManager.ClearCache();
		});
		return Task.CompletedTask;
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		_listenerDisposable?.Dispose();
		return Task.CompletedTask;
	}
}
