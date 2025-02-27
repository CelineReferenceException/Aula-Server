using System.Collections.Concurrent;
using System.Threading.RateLimiting;

namespace Aula.Server.Common.RateLimiting;

internal sealed class RateLimiterManager
{
	private readonly ConcurrentDictionary<DefaultKeyType, RateLimiter> _rateLimiters = new();

	internal TimeSpan MaximumReplenishmentPeriod { get; private set; } = TimeSpan.Zero;

	internal RateLimiter GetOrAdd(RateLimitPartition<DefaultKeyType> partition)
	{
		var rateLimiter = _rateLimiters.GetOrAdd(partition.PartitionKey, static (_, p) => p.Factory(p.PartitionKey), partition);
		if (rateLimiter is ReplenishingRateLimiter replenishingRateLimiter &&
		    replenishingRateLimiter.ReplenishmentPeriod > MaximumReplenishmentPeriod)
		{
			MaximumReplenishmentPeriod = replenishingRateLimiter.ReplenishmentPeriod;
		}

		return rateLimiter;
	}

	internal void RemoveUnusedRateLimiters(TimeSpan idleDuration)
	{
		foreach (var entry in _rateLimiters.Where(r => r.Value.IdleDuration > idleDuration))
		{
			entry.Value.Dispose();
			_ = _rateLimiters.TryRemove(entry);
		}
	}
}
