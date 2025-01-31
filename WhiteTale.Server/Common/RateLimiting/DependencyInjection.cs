using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace WhiteTale.Server.Common.RateLimiting;

internal static class DependencyInjection
{
	internal static IServiceCollection AddRateLimiters(this IServiceCollection services)
	{
		_ = services.AddOptions<RateLimitOptions>(RateLimitPolicyNames.Global)
			.BindConfiguration($"RateLimiters:{nameof(RateLimitPolicyNames.Global)}")
			.ValidateDataAnnotations()
			.ValidateOnStart();

		_ = services.PostConfigure<RateLimitOptions>(RateLimitPolicyNames.Global, options =>
		{
			options.WindowMilliseconds ??= 1000;
			options.PermitLimit ??= 30;
		});

		_ = services.AddRateLimiter(options =>
		{
			options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

			_ = options.AddPolicy(RateLimitPolicyNames.Global, httpContext =>
			{
				var userManager = ServiceProviderServiceExtensions.GetRequiredService<UserManager<User>>(httpContext.RequestServices);

				var userId = userManager.GetUserId(httpContext.User);
				var partitionKey = userId ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? String.Empty;

				var rateLimit = httpContext.RequestServices
					.GetRequiredService<IOptionsSnapshot<RateLimitOptions>>()
					.Get(RateLimitPolicyNames.Global);
				var permitLimit = rateLimit.PermitLimit!.Value;
				var window = TimeSpan.FromMilliseconds(rateLimit.WindowMilliseconds!.Value);

				return RateLimitPartition.GetFixedWindowLimiter(partitionKey,
					_ => new FixedWindowRateLimiterOptions
					{
						PermitLimit = permitLimit,
						Window = window,
						AutoReplenishment = true,
					});
			});

			_ = options.AddPolicy(RateLimitPolicyNames.NoConcurrency, httpContext =>
			{
				var userManager = ServiceProviderServiceExtensions.GetRequiredService<UserManager<User>>(httpContext.RequestServices);
				var request = httpContext.Request;

				var userId = userManager.GetUserId(httpContext.User);
				var partitionKey = userId ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? String.Empty;
				partitionKey += $"{request.Method}{request.Scheme}{request.Host}{request.PathBase}{request.Path}";

				return RateLimitPartition.GetConcurrencyLimiter(partitionKey,
					_ => new ConcurrencyLimiterOptions
					{
						PermitLimit = 1,
					});
			});
		});

		return services;
	}
}
