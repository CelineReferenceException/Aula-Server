using System.Collections.Concurrent;
using System.Threading.RateLimiting;

namespace Aula.Server.Common.RateLimiting;

internal sealed class RateLimiterManager
{
	private readonly ConcurrentDictionary<DefaultKeyType, RateLimiter> _rateLimiters = new();

	internal RateLimiter GetOrAdd(RateLimitPartition<DefaultKeyType> partition)
	{
		var rateLimiter = _rateLimiters.GetOrAdd(partition.PartitionKey, static (_, p) => p.Factory(p.PartitionKey), partition);

		return rateLimiter;
	}

	internal Int32 RemoveUnusedReplenishingRateLimiters()
	{
		var unusedRateLimiters = _rateLimiters
			.Where(r => r.Value is ExtendedReplenishingRateLimiter er &&
			            er.FirstWindowAcquireDateTime + er.ReplenishmentPeriod * 2 < DateTime.UtcNow)
			.ToList();

		foreach (var entry in unusedRateLimiters)
		{
			_ = _rateLimiters.TryRemove(entry);
			entry.Value.Dispose();
		}

		return unusedRateLimiters.Count;
	}

	internal Int32 ClearCache()
	{
		var count = _rateLimiters.Count;
		foreach (var entry in _rateLimiters)
		{
			_ = _rateLimiters.TryRemove(entry);
			entry.Value.Dispose();
		}

		return count;
	}
}
