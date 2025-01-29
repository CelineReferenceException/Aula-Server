namespace WhiteTale.Server.Common.AuthorizationRequirements;

internal static class DependencyInjection
{
	internal static IServiceCollection AddAuthorizationRequirements(this IServiceCollection services)
	{
		_ = services.AddAuthorizationBuilder()
			.AddPermissionsPolicy();

		return services;
	}
}
