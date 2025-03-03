namespace Aula.Server.Features.Users;

internal static class DependencyInjection
{
	internal static IServiceCollection AddUserPresenceServices(this IServiceCollection services)
	{
		_ = services.AddHostedService<ResetPresencesHostedService>();

		return services;
	}
}
