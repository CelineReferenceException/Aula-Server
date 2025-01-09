using Microsoft.AspNetCore.Authorization;

namespace WhiteTale.Server.Common.Identity;

internal static class IdentityAuthorizationPoliciesExtensions
{
	internal static AuthorizationBuilder AddIdentityBearerTokenPolicy(this AuthorizationBuilder builder)
	{
		_ = builder.AddPolicy(IdentityAuthorizationPolicyNames.BearerToken, policy =>
		{
			_ = policy.RequireAuthenticatedUser();
			_ = policy.AddAuthenticationSchemes(IdentityConstants.BearerScheme);
		});

		return builder;
	}
}
