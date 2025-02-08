using WhiteTale.Server.Features.Identity.Endpoints;
using WhiteTale.Server.Features.Users;

namespace WhiteTale.Server.Features;

internal static class DependencyInjection
{
	internal static IServiceCollection AddFeatures(this IServiceCollection services)
	{
		_ = services.AddIdentityFeatures();
		_ = services.AddUserFeatures();

		return services;
	}
}
