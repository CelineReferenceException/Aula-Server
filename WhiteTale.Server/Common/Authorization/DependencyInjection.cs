namespace WhiteTale.Server.Common.Authorization;

internal static class DependencyInjection
{
	internal static IServiceCollection AddApplicationAuthorization(this IServiceCollection services)
	{
		_ = services.AddAuthorizationBuilder()
			.AddPolicy(AuthorizationPolicyNames.AuthenticatedUser, policy =>
			{
				_ = policy.RequireAuthenticatedUser();
				_ = policy.AddAuthenticationSchemes(AuthenticationSchemeNames.BearerToken);
			});

		return services;
	}
}
