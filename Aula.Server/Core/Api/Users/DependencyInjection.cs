namespace Aula.Server.Core.Api.Users;

internal static class DependencyInjection
{
	internal static IServiceCollection AddUserApi(this IServiceCollection services)
	{
		_ = services.AddUserPresenceServices();
		return services;
	}

	internal static IServiceCollection AddUserPresenceServices(this IServiceCollection services)
	{
		_ = services.AddHostedService<ResetPresencesService>();

		return services;
	}
}
