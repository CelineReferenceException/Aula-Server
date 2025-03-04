namespace Aula.Server.Common.Gateway;

internal static class GatewayRateLimitPolicies
{
	private const String Prefix = nameof(GatewayRateLimitPolicies);

	internal const String Gateway = $"{Prefix}.{nameof(Gateway)}";
}
