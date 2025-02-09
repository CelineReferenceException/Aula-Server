using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;

namespace WhiteTale.Server.Common.Authorization;

internal static class AuthenticatedUserPolicyExtensions
{
	private const String PolicyName = "AuthenticatedUserPolicy";

	internal static TBuilder RequireAuthenticatedUser<TBuilder>(this TBuilder builder)
		where TBuilder : IEndpointConventionBuilder
	{
		return builder.RequireAuthorization(PolicyName);
	}

	internal static AuthorizationBuilder AddAuthenticatedUserPolicy(this AuthorizationBuilder builder)
	{
		_ = builder.AddPolicy(PolicyName, policy =>
		{
			_ = policy.RequireAuthenticatedUser();
			_ = policy.AddAuthenticationSchemes(AuthenticationSchemeNames.BearerToken);
		});

		return builder;
	}
}
