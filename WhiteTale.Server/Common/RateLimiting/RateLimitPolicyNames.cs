namespace WhiteTale.Server.Common.RateLimiting;

internal static class RateLimitPolicyNames
{
	internal const String Global = nameof(Global);

	internal const String Strict = nameof(Strict);

	internal const String NoConcurrency = nameof(NoConcurrency);
}
