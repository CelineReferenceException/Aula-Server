using System.Threading.RateLimiting;

namespace Aula.Server.Common.RateLimiting;

internal sealed class ExtendedReplenishingRateLimiter : ReplenishingRateLimiter
{
	private readonly ReplenishingRateLimiter _underlyingRateLimiter;

	internal ExtendedReplenishingRateLimiter(ReplenishingRateLimiter underlyingRateLimiter)
	{
		_underlyingRateLimiter = underlyingRateLimiter;
	}

	public override TimeSpan? IdleDuration => _underlyingRateLimiter.IdleDuration;

	public override Boolean IsAutoReplenishing => _underlyingRateLimiter.IsAutoReplenishing;

	public override TimeSpan ReplenishmentPeriod => _underlyingRateLimiter.ReplenishmentPeriod;

	internal DateTime? FirstWindowAcquireDateTime { get; private set; }

	internal DateTime? ReplenishmentDateTime => FirstWindowAcquireDateTime + _underlyingRateLimiter.ReplenishmentPeriod;

	protected override async ValueTask<RateLimitLease> AcquireAsyncCore(Int32 permitCount, CancellationToken cancellationToken)
	{
		ReplenishIfAcceptable();
		FirstWindowAcquireDateTime ??= DateTime.UtcNow;
		return await _underlyingRateLimiter.AcquireAsync(permitCount, cancellationToken);
	}

	protected override RateLimitLease AttemptAcquireCore(Int32 permitCount)
	{
		ReplenishIfAcceptable();
		FirstWindowAcquireDateTime ??= DateTime.UtcNow;
		return _underlyingRateLimiter.AttemptAcquire(permitCount);
	}

	public override RateLimiterStatistics? GetStatistics()
	{
		return _underlyingRateLimiter.GetStatistics();
	}

	public override Boolean TryReplenish()
	{
		var replenished = _underlyingRateLimiter.TryReplenish();
		if (replenished)
		{
			FirstWindowAcquireDateTime = null;
		}

		return replenished;
	}

	private void ReplenishIfAcceptable()
	{
		var now = DateTime.UtcNow;
		if (ReplenishmentDateTime is not null &&
		    FirstWindowAcquireDateTime is not null &&
		    now - FirstWindowAcquireDateTime > ReplenishmentPeriod)
		{
			_ = TryReplenish();
		}
	}
}
