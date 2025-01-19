namespace WhiteTale.Server.Common.RateLimiting;

internal sealed class RateLimitOptions
{
	public Int32? WindowMilliseconds { get; set; }

	public Int32? PermitLimit { get; set; }
}
