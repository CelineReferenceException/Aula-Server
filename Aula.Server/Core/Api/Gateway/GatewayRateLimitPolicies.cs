namespace Aula.Server.Core.Api.Gateway;

internal static class GatewayRateLimitPolicies
{
	internal const String Gateway = $"{Prefix}.{nameof(Gateway)}";
	private const String Prefix = nameof(GatewayRateLimitPolicies);
}
