namespace WhiteTale.Server.Common.Authorization;

internal static class AuthorizationPolicyNames
{
	private const String Prefix = nameof(AuthorizationPolicyNames);
	internal const String AuthenticatedUser = $"{Prefix}.{nameof(AuthenticatedUser)}";
}
