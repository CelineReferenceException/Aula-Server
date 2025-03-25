namespace Aula.Server.Core.Features.Gateway;

internal static class GatewayRateLimitPolicies
{
	internal const String Gateway = $"{Prefix}.{nameof(Gateway)}";
	private const String Prefix = nameof(GatewayRateLimitPolicies);
}
