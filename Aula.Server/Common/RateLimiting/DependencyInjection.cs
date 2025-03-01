using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Aula.Server.Common.RateLimiting;

internal static class DependencyInjection
{
	internal static IServiceCollection AddCustomRateLimiter(
		this IServiceCollection services,
		Action<RateLimiterOptions> configureOptions)
	{
		_ = services.AddCustomRateLimiter();
		_ = services.Configure(configureOptions);
		return services;
	}

	internal static IServiceCollection AddCustomRateLimiter(this IServiceCollection services)
	{
		services.TryAddSingleton<RateLimiterManager>();
		_ = services.AddHostedService<ClearIdleRateLimitersService>();
		_ = services.AddHostedService<ClearRateLimitersOnConfigurationUpdateService>();

		_ = services.AddOptions<RateLimitOptions>("Global")
			.BindConfiguration("RateLimiters:Global")
			.ValidateDataAnnotations()
			.ValidateOnStart();

		_ = services.PostConfigure<RateLimitOptions>("Global", options =>
		{
			options.WindowMilliseconds ??= 1000;
			options.PermitLimit ??= 30;
		});

		_ = services.Configure<RateLimiterOptions>(options =>
		{
			options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

			_ = options.SetGlobalPolicy(httpContext =>
			{
				var userManager = ServiceProviderServiceExtensions.GetRequiredService<UserManager>(httpContext.RequestServices);

				var userId = userManager.GetUserId(httpContext.User);
				var partitionKey = userId.HasValue
					? userId.Value.ToString()
					: httpContext.Connection.RemoteIpAddress?.ToString();
				if (partitionKey is null ||
				    partitionKey.Length == 0)
				{
					throw new NotImplementedException("Fallback not implemented.");
				}

				return RateLimitPartitionExtensions.GetExtendedFixedWindowRateLimiter(partitionKey, _ =>
				{
					var rateLimit = httpContext.RequestServices
						.GetRequiredService<IOptionsSnapshot<RateLimitOptions>>()
						.Get("Global");

					return new FixedWindowRateLimiterOptions
					{
						PermitLimit = rateLimit.PermitLimit!.Value,
						Window = TimeSpan.FromMilliseconds(rateLimit.WindowMilliseconds!.Value),
					};
				});
			});
		});

		return services;
	}

	internal static TBuilder UseCustomRateLimiting<TBuilder>(this TBuilder builder) where TBuilder : IApplicationBuilder
	{
		_ = builder.Use(async (httpContext, next) =>
		{
			var endpoint = httpContext.GetEndpoint();
			if (endpoint is null)
			{
				_ = next(httpContext);
				return;
			}

			var ignoreRateLimitingAttribute = endpoint.Metadata.GetMetadata<IgnoreRateLimitingAttribute>();
			if (ignoreRateLimitingAttribute is not null)
			{
				_ = next(httpContext);
				return;
			}

			var options = httpContext.RequestServices.GetRequiredService<IOptions<RateLimiterOptions>>()
				.Value;
			if (options.GlobalPolicy is not null)
			{
				var globalLease = await ExecuteRateLimiterPolicyAsync(options.GlobalPolicy, RateLimitTargetType.Global, httpContext);
				if (!globalLease.IsAcquired)
				{
					httpContext.Response.StatusCode = options.RejectionStatusCode;
					return;
				}
			}

			var rateLimit = endpoint.Metadata.GetMetadata<RequireRateLimitingAttribute>();
			if (rateLimit is null)
			{
				_ = next(httpContext);
				return;
			}

			if (!options.PolicyMap.TryGetValue(rateLimit.PolicyName, out var policy))
			{
				throw new InvalidOperationException(
					$"This endpoint requires a rate limiting policy with name {rateLimit.PolicyName}, but no such policy exists.");
			}

			var lease = await ExecuteRateLimiterPolicyAsync(policy, RateLimitTargetType.Endpoint, httpContext);
			if (!lease.IsAcquired)
			{
				httpContext.Response.StatusCode = options.RejectionStatusCode;
				return;
			}

			_ = next(httpContext);
		});

		return builder;
	}

	internal static TBuilder IgnoreRateLimiting<TBuilder>(this TBuilder builder) where TBuilder : IEndpointConventionBuilder
	{
		return builder.WithMetadata(new IgnoreRateLimitingAttribute());
	}

	internal static TBuilder ApplyRateLimiting<TBuilder>(this TBuilder builder, String policyName)
		where TBuilder : IEndpointConventionBuilder
	{
		return builder.WithMetadata(new RequireRateLimitingAttribute(policyName));
	}

	private static async ValueTask<RateLimitLease> ExecuteRateLimiterPolicyAsync(
		RateLimiterPolicy policy,
		RateLimitTargetType type,
		HttpContext httpContext)
	{
		var rateLimitOptions = httpContext.RequestServices
			.GetRequiredService<IOptionsSnapshot<RateLimitOptions>>()
			.Get("Global");
		httpContext.Response.Headers.Append($"X-RateLimit-{type}-Limit", rateLimitOptions.PermitLimit.ToString());

		var rateLimiterManager = httpContext.RequestServices.GetRequiredService<RateLimiterManager>();
		var rateLimiter = rateLimiterManager.GetOrAdd(policy.GetPartition(httpContext));

		var rateLimitLease = rateLimiter.AttemptAcquire();

		var statistics = rateLimiter.GetStatistics();
		if (statistics is not null)
		{
			httpContext.Response.Headers.Append($"X-RateLimit-{type}-Remaining",
				statistics.CurrentAvailablePermits.ToString());
		}

		if (rateLimiter is ExtendedReplenishingRateLimiter { ReplenishmentDateTime: not null, } replenishingRateLimiter)
		{
			var replenishmentDateTime = replenishingRateLimiter.ReplenishmentDateTime.Value;
			httpContext.Response.Headers.Append($"X-RateLimit-{type}-ResetDateTime", replenishmentDateTime.ToString("O"));
		}

		if (!rateLimitLease.IsAcquired &&
		    policy.OnRejected is not null)
		{
			await policy.OnRejected(new OnRejectedContext
			{
				HttpContext = httpContext,
				Lease = rateLimitLease,
			}, httpContext.RequestAborted);
		}

		return rateLimitLease;
	}

	private enum RateLimitTargetType
	{
		Global,
		Endpoint,
	}
}
