using System.ComponentModel.DataAnnotations;

namespace WhiteTale.Server.Common.RateLimiting;

internal sealed class RateLimitOptions
{
	[Required]
	public required Int32 WindowMilliseconds { get; set; }

	[Required]
	public required Int32 PermitLimit { get; set; }
}
