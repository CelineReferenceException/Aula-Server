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
}
