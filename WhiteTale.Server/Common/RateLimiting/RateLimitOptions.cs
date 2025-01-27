namespace WhiteTale.Server.Common.RateLimiting;

/// <summary>
///     Configurations for a rate limit profile.
/// </summary>
internal sealed class RateLimitOptions
{
	/// <summary>
	///     The window of the rate limit in milliseconds.
	/// </summary>
	public Int32? WindowMilliseconds { get; set; }

	/// <summary>
	///     How many requests can be made during the rate limit window.
	/// </summary>
	public Int32? PermitLimit { get; set; }
}
