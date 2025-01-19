namespace WhiteTale.Server.Features.Identity;

internal static class IdentityRateLimitPolicyNames
{
	internal const String ForgotPassword = $"{nameof(IdentityRateLimitPolicyNames)}:{nameof(ForgotPassword)}";
}
