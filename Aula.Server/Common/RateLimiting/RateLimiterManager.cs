using System.Collections.Concurrent;
using System.Threading.RateLimiting;

namespace Aula.Server.Common.RateLimiting;

internal sealed class RateLimiterManager
{
	private readonly ConcurrentDictionary<DefaultKeyType, RateLimiter> _rateLimiterCache = new();

	internal RateLimiter GetOrAdd(RateLimitPartition<DefaultKeyType> partition)
	{
		var rateLimiter = _rateLimiterCache.GetOrAdd(partition.PartitionKey, static (_, p) => p.Factory(p.PartitionKey), partition);

		return rateLimiter;
	}

	internal Int32 ClearIdleRateLimitersFromCache()
	{
		var unusedRateLimiters = _rateLimiterCache
			.Where(r => r.Value is ExtendedReplenishingRateLimiter er &&
			            er.FirstWindowAcquireDateTime + er.ReplenishmentPeriod * 2 < DateTime.UtcNow)
			.ToList();

		foreach (var entry in unusedRateLimiters)
		{
			_ = _rateLimiterCache.TryRemove(entry);
			entry.Value.Dispose();
		}

		return unusedRateLimiters.Count;
	}

	internal Int32 ClearCache()
	{
		var count = _rateLimiterCache.Count;
		foreach (var entry in _rateLimiterCache)
		{
			_ = _rateLimiterCache.TryRemove(entry);
			entry.Value.Dispose();
		}

		return count;
	}
}
