using Microsoft.AspNetCore.Authorization;

namespace WhiteTale.Server.Common.Authorization;

internal static class BearerTokenPolicyExtensions
{
	internal static AuthorizationBuilder AddBearerTokenPolicy(this AuthorizationBuilder builder)
	{
		_ = builder.AddPolicy(AuthorizationPolicyNames.BearerToken, policy =>
		{
			_ = policy.RequireAuthenticatedUser();
			_ = policy.AddAuthenticationSchemes(IdentityConstants.BearerScheme);
		});

		return builder;
	}
}
