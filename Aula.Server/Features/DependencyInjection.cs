using Aula.Server.Features.Identity.Endpoints;
using Aula.Server.Features.Users;

namespace Aula.Server.Features;

internal static class DependencyInjection
{
	internal static IServiceCollection AddFeatures(this IServiceCollection services)
	{
		_ = services.AddIdentityFeatures();
		_ = services.AddUserFeatures();

		return services;
	}
}
