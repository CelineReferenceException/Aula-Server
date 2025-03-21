﻿using Microsoft.AspNetCore.WebSockets;

namespace Aula.Server.Core.Gateway;

internal static class DependencyInjection
{
	internal static IServiceCollection AddGateway(this IServiceCollection services)
	{
		_ = services.AddOptions<GatewayOptions>()
			.BindConfiguration(GatewayOptions.SectionName)
			.ValidateDataAnnotations()
			.ValidateOnStart();

		_ = services.AddSingleton<GatewayService>();
		_ = services.AddHostedService<ExpiredSessionsCleanerService>();

		_ = services.AddWebSockets(options =>
		{
			options.KeepAliveInterval = TimeSpan.Zero;
			options.KeepAliveTimeout = TimeSpan.FromSeconds(60);
		});

		return services;
	}
}
