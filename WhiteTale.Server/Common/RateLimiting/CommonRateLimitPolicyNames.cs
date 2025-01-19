namespace WhiteTale.Server.Common.RateLimiting;

internal static class CommonRateLimitPolicyNames
{
	internal const String Global = $"{nameof(CommonRateLimitPolicyNames)}:{nameof(Global)}";

	internal const String NoConcurrency = $"{nameof(CommonRateLimitPolicyNames)}:{nameof(NoConcurrency)}";
}
