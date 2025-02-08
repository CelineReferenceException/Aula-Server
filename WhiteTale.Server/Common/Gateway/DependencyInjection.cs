using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.Extensions.Options;

namespace WhiteTale.Server.Common.Gateway;

internal static class DependencyInjection
{
	internal static IServiceCollection AddGateway(this IServiceCollection services)
	{
		_ = services.AddSingleton<GatewayService>();
		_ = services.AddHostedService<RemoveExpiredSessionsHostedService>();

		_ = services.AddWebSockets(options =>
		{
			options.KeepAliveInterval = TimeSpan.Zero;
			options.KeepAliveTimeout = TimeSpan.FromSeconds(60);
		});

		_ = services.AddOptions<RateLimitOptions>(GatewayRateLimitPolicyNames.Default)
			.BindConfiguration($"RateLimiters:{nameof(GatewayRateLimitPolicyNames.Default)}")
			.ValidateDataAnnotations()
			.ValidateOnStart();

		_ = services.PostConfigure<RateLimitOptions>(GatewayRateLimitPolicyNames.Default, options =>
		{
			options.WindowMilliseconds ??= (Int32)TimeSpan.FromHours(24).TotalMilliseconds;
			options.PermitLimit ??= 1000;
		});

		_ = services.AddRateLimiter(options =>
		{
			options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

			_ = options.AddPolicy(GatewayRateLimitPolicyNames.Default, httpContext =>
			{
				var userManager = ServiceProviderServiceExtensions.GetRequiredService<UserManager>(httpContext.RequestServices);

				var userId = userManager.GetUserId(httpContext.User);
				var partitionKey = userId.ToString() ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? String.Empty;

				var rateLimitOptions = httpContext.RequestServices
					.GetRequiredService<IOptionsSnapshot<RateLimitOptions>>()
					.Get(GatewayRateLimitPolicyNames.Default);

				var rateLimiterOptions = new FixedWindowRateLimiterOptions
				{
					PermitLimit = rateLimitOptions.PermitLimit!.Value,
					Window = TimeSpan.FromMilliseconds(rateLimitOptions.WindowMilliseconds!.Value),
					AutoReplenishment = true,
				};

				return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ => rateLimiterOptions);
			});
		});

		return services;
	}
}
