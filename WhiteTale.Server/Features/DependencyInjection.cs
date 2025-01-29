using WhiteTale.Server.Features.Gateway;
using WhiteTale.Server.Features.Identity;
using WhiteTale.Server.Features.Rooms;
using WhiteTale.Server.Features.Users;

namespace WhiteTale.Server.Features;

internal static class DependencyInjection
{
	internal static IServiceCollection AddFeatures(this IServiceCollection services)
	{
		_ = services.AddGateway();
		_ = services.AddIdentityFeatures();
		_ = services.AddUserFeatures();
		_ = services.AddRoomFeatures();

		return services;
	}
}
