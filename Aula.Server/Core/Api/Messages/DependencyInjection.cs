using System.Diagnostics;
using System.Threading.RateLimiting;
using Aula.Server.Common.Identity;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

namespace Aula.Server.Core.Api.Messages;

internal static class DependencyInjection
{
	internal static IServiceCollection AddMessageApi(this IServiceCollection services)
	{
		_ = services.Configure<JsonOptions>(options => options.SerializerOptions.TypeInfoResolverChain.Add(MessageJsonContext.Default));
		_ = services.AddMessageEndpointRateLimiters();
		return services;
	}

	private static IServiceCollection AddMessageEndpointRateLimiters(this IServiceCollection services)
	{
		_ = services.AddOptions<RateLimitOptions>(MessageRateLimitingPolicies.SendMessage)
			.BindConfiguration($"RateLimiters:{nameof(MessageRateLimitingPolicies.SendMessage)}")
			.ValidateDataAnnotations()
			.ValidateOnStart();

		_ = services.PostConfigure<RateLimitOptions>(MessageRateLimitingPolicies.SendMessage, options =>
		{
			options.WindowMilliseconds ??= 60000;
			options.PermitLimit ??= 30;
		});

		_ = services.AddCustomRateLimiter(options =>
		{
			_ = options.AddPolicy(MessageRateLimitingPolicies.SendMessage, httpContext =>
			{
				var userManager = httpContext.RequestServices.GetRequiredService<UserManager>();

				var userId = userManager.GetUserId(httpContext.User) ??
				             throw new UnreachableException("User must be authenticated before applying the rate limit.");

				if (!httpContext.Request.RouteValues.TryGetValue("roomId", out var roomId))
				{
					throw new UnreachableException("roomId not found. Maybe the route value name has changed.");
				}

				var partitionKey = $"{userId}.{roomId}";

				var rateLimit = httpContext.RequestServices
					.GetRequiredService<IOptionsSnapshot<RateLimitOptions>>()
					.Get(MessageRateLimitingPolicies.SendMessage);

				return RateLimitPartitionExtensions.GetExtendedFixedWindowRateLimiter(partitionKey, _ => new FixedWindowRateLimiterOptions
				{
					PermitLimit = rateLimit.PermitLimit!.Value,
					Window = TimeSpan.FromMilliseconds(rateLimit.WindowMilliseconds!.Value),
				});
			});
		});

		_ = services.AddOptions<RateLimitOptions>(MessageRateLimitingPolicies.RemoveMessage)
			.BindConfiguration($"RateLimiters:{nameof(MessageRateLimitingPolicies.RemoveMessage)}")
			.ValidateDataAnnotations()
			.ValidateOnStart();

		_ = services.PostConfigure<RateLimitOptions>(MessageRateLimitingPolicies.RemoveMessage, options =>
		{
			options.WindowMilliseconds ??= 60000;
			options.PermitLimit ??= 30;
		});

		_ = services.AddCustomRateLimiter(options =>
		{
			_ = options.AddPolicy(MessageRateLimitingPolicies.RemoveMessage, httpContext =>
			{
				var userManager = httpContext.RequestServices.GetRequiredService<UserManager>();

				var userId = userManager.GetUserId(httpContext.User) ??
				             throw new UnreachableException("User must be authenticated before applying the rate limit.");

				if (!httpContext.Request.RouteValues.TryGetValue("roomId", out var roomId))
				{
					throw new UnreachableException("roomId not found. Maybe the route value name has changed.");
				}

				var partitionKey = $"{userId}.{roomId}";

				var rateLimit = httpContext.RequestServices
					.GetRequiredService<IOptionsSnapshot<RateLimitOptions>>()
					.Get(MessageRateLimitingPolicies.RemoveMessage);

				return RateLimitPartitionExtensions.GetExtendedFixedWindowRateLimiter(partitionKey, _ => new FixedWindowRateLimiterOptions
				{
					PermitLimit = rateLimit.PermitLimit!.Value,
					Window = TimeSpan.FromMilliseconds(rateLimit.WindowMilliseconds!.Value),
				});
			});
		});

		return services;
	}
}
