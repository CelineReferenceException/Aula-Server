namespace WhiteTale.Server.Common.Authorization;

internal static class DependencyInjection
{
	internal static IServiceCollection AddApplicationAuthorization(this IServiceCollection services)
	{
		_ = services.AddAuthorizationBuilder()
			.AddAuthenticatedUserPolicy()
			.AddBanPolicy()
			.AddPermissionsPolicy();

		return services;
	}
}
