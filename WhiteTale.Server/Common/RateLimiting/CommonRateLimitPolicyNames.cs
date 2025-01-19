namespace WhiteTale.Server.Common.RateLimiting;

internal static class CommonRateLimitPolicyNames
{
	private const String Prefix = nameof(CommonRateLimitPolicyNames);

	internal const String Global = $"{Prefix}.{nameof(Global)}";

	internal const String NoConcurrency = $"{Prefix}.{nameof(NoConcurrency)}";
}
