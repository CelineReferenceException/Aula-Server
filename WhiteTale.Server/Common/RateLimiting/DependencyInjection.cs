using System.Diagnostics;
using System.Threading.RateLimiting;
using Microsoft.Extensions.Options;
using WhiteTale.Server.Domain.Users;

namespace WhiteTale.Server.Common.RateLimiting;

internal static class DependencyInjection
{
	internal static IServiceCollection AddRateLimiters(this IServiceCollection services)
	{
		_ = services.AddOptions<RateLimitOptions>(CommonRateLimitPolicyNames.Global)
			.BindConfiguration($"RateLimiters:{nameof(CommonRateLimitPolicyNames.Global)}")
			.ValidateDataAnnotations()
			.ValidateOnStart();

		_ = services.PostConfigure<RateLimitOptions>(CommonRateLimitPolicyNames.Global, options =>
		{
			options.WindowMilliseconds ??= 1000;
			options.PermitLimit ??= 30;
		});

		_ = services.AddRateLimiter(options =>
		{
			options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

			_ = options.AddPolicy(CommonRateLimitPolicyNames.Global, httpContext =>
			{
				var userManager = httpContext.RequestServices.GetRequiredService<UserManager<User>>();

				var userId = userManager.GetUserId(httpContext.User);
				var partitionKey = userId ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? String.Empty;

				var rateLimit = httpContext.RequestServices
					.GetRequiredService<IOptionsSnapshot<RateLimitOptions>>()
					.Get(CommonRateLimitPolicyNames.Global);
				var permitLimit = rateLimit.PermitLimit!.Value;
				var window = TimeSpan.FromMilliseconds(rateLimit.WindowMilliseconds!.Value);

				return RateLimitPartition.GetFixedWindowLimiter(partitionKey,
					_ => new FixedWindowRateLimiterOptions
					{
						PermitLimit = permitLimit,
						Window = window,
						AutoReplenishment = true
					});
			});

			_ = options.AddPolicy(CommonRateLimitPolicyNames.NoConcurrency, httpContext =>
			{
				var userManager = ServiceProviderServiceExtensions.GetRequiredService<UserManager<User>>(httpContext.RequestServices);
				var request = httpContext.Request;

				var userId = userManager.GetUserId(httpContext.User);
				var partitionKey = userId ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? String.Empty;
				partitionKey += $"{request.Method}{request.Scheme}{request.Host}{request.PathBase}{request.Path}";

				return RateLimitPartition.GetConcurrencyLimiter(partitionKey,
					_ => new ConcurrencyLimiterOptions { PermitLimit = 1 });
			});
		});

		return services;
	}
}
