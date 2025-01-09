using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace WhiteTale.Server.Common.RateLimiting;

internal sealed class RateLimitOptions
{
	[Required]
	[NotNull]
	public required Int32? WindowMilliseconds { get; set; }

	[Required]
	[NotNull]
	public required Int32? PermitLimit { get; set; }
}
