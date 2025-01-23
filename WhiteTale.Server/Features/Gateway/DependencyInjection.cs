using Microsoft.AspNetCore.WebSockets;

namespace WhiteTale.Server.Features.Gateway;

internal static class DependencyInjection
{
	internal static IServiceCollection AddGateway(this IServiceCollection services)
	{
		_ = services.AddHostedService<RemoveExpiredSessionsHostedService>();
		_ = services.AddWebSockets(options =>
		{
			options.KeepAliveInterval = TimeSpan.Zero;
			options.KeepAliveTimeout = TimeSpan.FromSeconds(60);
		});

		return services;
	}
}
