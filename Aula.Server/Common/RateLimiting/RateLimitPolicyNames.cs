namespace Aula.Server.Common.RateLimiting;

internal static class RateLimitPolicyNames
{
	private const String Prefix = nameof(RateLimitPolicyNames);

	internal const String Global = $"{Prefix}.{nameof(Global)}";

	internal const String NoConcurrency = $"{Prefix}.{nameof(NoConcurrency)}";
}
