namespace WhiteTale.Server.Common.Identity;

internal static class IdentityAuthorizationPolicyNames
{
	private const String Prefix = nameof(IdentityAuthorizationPolicyNames);

	internal const String BearerToken = $"{Prefix}.{nameof(BearerToken)}";
}
