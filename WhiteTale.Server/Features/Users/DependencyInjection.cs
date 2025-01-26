namespace WhiteTale.Server.Features.Users;

internal static class DependencyInjection
{
	internal static IServiceCollection AddUserFeatures(this IServiceCollection services)
	{
		_ = services.AddHostedService<ResetPresencesHostedService>();

		return services;
	}
}
