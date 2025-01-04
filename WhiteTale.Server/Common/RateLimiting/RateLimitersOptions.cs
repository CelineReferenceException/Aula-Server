using System.ComponentModel.DataAnnotations;

namespace WhiteTale.Server.Common.RateLimiting;

internal sealed class RateLimitersOptions
{
	internal const String SectionName = "RateLimiters";

	[Required]
	public required RateLimitOptions Global { get; set; }

	[Required]
	public required RateLimitOptions Strict { get; set; }
}
