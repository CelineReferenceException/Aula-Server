using System.Threading.RateLimiting;
using Aula.Server.Common.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Aula.Server.Features.Gateway.Endpoints;

internal static class DependencyInjection
{
	internal static IServiceCollection AddGatewayEndpointRateLimiters(this IServiceCollection services)
	{
		_ = services.AddOptions<RateLimitOptions>(GatewayRateLimitPolicies.Gateway)
			.BindConfiguration($"RateLimiters:{nameof(GatewayRateLimitPolicies.Gateway)}")
			.ValidateDataAnnotations()
			.ValidateOnStart();

		_ = services.PostConfigure<RateLimitOptions>(GatewayRateLimitPolicies.Gateway, options =>
		{
			options.WindowMilliseconds ??= (Int32)TimeSpan.FromHours(24).TotalMilliseconds;
			options.PermitLimit ??= 1000;
		});

		_ = services.AddCustomRateLimiter(options =>
		{
			options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

			_ = options.AddPolicy(GatewayRateLimitPolicies.Gateway, httpContext =>
			{
				var userManager = ServiceProviderServiceExtensions.GetRequiredService<UserManager>(httpContext.RequestServices);

				var userId = userManager.GetUserId(httpContext.User);
				var partitionKey = userId.ToString() ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? String.Empty;

				return RateLimitPartitionExtensions.GetExtendedFixedWindowRateLimiter(partitionKey, _ =>
				{
					var rateLimitOptions = httpContext.RequestServices
						.GetRequiredService<IOptionsSnapshot<RateLimitOptions>>()
						.Get(GatewayRateLimitPolicies.Gateway);

					return new FixedWindowRateLimiterOptions
					{
						PermitLimit = rateLimitOptions.PermitLimit!.Value,
						Window = TimeSpan.FromMilliseconds(rateLimitOptions.WindowMilliseconds!.Value),
					};
				});
			});
		});

		return services;
	}
}
