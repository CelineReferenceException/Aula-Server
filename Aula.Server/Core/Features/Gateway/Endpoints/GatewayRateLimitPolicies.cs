namespace Aula.Server.Core.Features.Gateway.Endpoints;

internal static class GatewayRateLimitPolicies
{
	private const String Prefix = nameof(GatewayRateLimitPolicies);

	internal const String Gateway = $"{Prefix}.{nameof(Gateway)}";
}
