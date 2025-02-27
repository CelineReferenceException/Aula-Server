using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;

namespace Aula.Server.Common.RateLimiting;

internal sealed class RateLimiterOptions
{
	private readonly Dictionary<String, RateLimiterPolicy> _policyMap = new(StringComparer.Ordinal);

	internal IReadOnlyDictionary<String, RateLimiterPolicy> PolicyMap => _policyMap;

	internal RateLimiterPolicy? GlobalPolicy { get; set; }

	/// <summary>
	///     Gets or sets a <see cref="Func{OnRejectedContext, CancellationToken, ValueTask}" /> that handles requests rejected by this middleware.
	/// </summary>
	internal Func<OnRejectedContext, CancellationToken, ValueTask>? OnRejected { get; set; }

	/// <summary>
	///     Gets or sets the default status code to set on the response when a request is rejected.
	///     Defaults to <see cref="StatusCodes.Status503ServiceUnavailable" />.
	/// </summary>
	/// <remarks>
	///     This status code will be set before <see cref="OnRejected" /> is called, so any status code set by
	///     <see cref="OnRejected" /> will "win" over this default.
	/// </remarks>
	internal Int32 RejectionStatusCode { get; set; } = StatusCodes.Status503ServiceUnavailable;

	/// <summary>
	///     Adds a new rate limiting policy with the given <paramref name="policyName" />
	/// </summary>
	/// <param name="policyName">The name to be associated with the given <see cref="RateLimiter" />.</param>
	/// <param name="partitioner">
	///     Method called every time an Acquire or WaitAsync call is made to determine what rate limiter to apply to the
	///     request.
	/// </param>
	internal RateLimiterOptions AddPolicy(
		String policyName,
		Func<HttpContext, RateLimitPartition<String>> partitioner)
	{
		if (_policyMap.ContainsKey(policyName))
		{
			throw new ArgumentException($"There already exists a policy with the name {policyName}.", nameof(policyName));
		}

		_policyMap.Add(policyName, new RateLimiterPolicy(partitioner, null));

		return this;
	}
}
