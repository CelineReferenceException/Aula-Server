namespace Aula.Server.Core.Features.Users;

internal static class DependencyInjection
{
	internal static IServiceCollection AddUserPresenceServices(this IServiceCollection services)
	{
		_ = services.AddHostedService<ResetPresencesService>();

		return services;
	}
}
