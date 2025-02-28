using System.Collections.Concurrent;
using System.Threading.RateLimiting;

namespace Aula.Server.Common.RateLimiting;

internal sealed partial class RateLimiterManager
{
	private readonly ILogger<RateLimiterManager> _logger;
	private readonly ConcurrentDictionary<DefaultKeyType, RateLimiter> _rateLimiters = new();

	public RateLimiterManager(ILogger<RateLimiterManager> logger)
	{
		_logger = logger;
	}

	internal RateLimiter GetOrAdd(RateLimitPartition<DefaultKeyType> partition)
	{
		var rateLimiter = _rateLimiters.GetOrAdd(partition.PartitionKey, static (_, p) => p.Factory(p.PartitionKey), partition);

		return rateLimiter;
	}

	internal void RemoveUnusedReplenishingRateLimiters()
	{
		foreach (var entry in _rateLimiters
			         .Where(r => r.Value is ExtendedReplenishingRateLimiter er &&
			                     er.FirstWindowAcquireDateTime + er.ReplenishmentPeriod * 2 < DateTime.UtcNow))
		{
			_ = _rateLimiters.TryRemove(entry);
			entry.Value.Dispose();
		}
	}

	internal void ClearCache()
	{
		var count = _rateLimiters.Count;
		foreach (var entry in _rateLimiters)
		{
			_ = _rateLimiters.TryRemove(entry);
			entry.Value.Dispose();
		}

		LogCacheClear(_logger, count);
	}

	[LoggerMessage(LogLevel.Information, Message = "Rate limiters cache clear, {count} elements removed.")]
	private static partial void LogCacheClear(ILogger logger, Int32 count);
}
