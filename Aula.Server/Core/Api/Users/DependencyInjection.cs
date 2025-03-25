namespace Aula.Server.Core.Api.Users;

internal static class DependencyInjection
{
	internal static IServiceCollection AddUserPresenceServices(this IServiceCollection services)
	{
		_ = services.AddHostedService<ResetPresencesService>();

		return services;
	}
}
