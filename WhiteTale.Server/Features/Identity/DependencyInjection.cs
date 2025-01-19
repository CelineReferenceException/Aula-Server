using System.Diagnostics;
using System.Threading.RateLimiting;
using Microsoft.Extensions.Options;
using WhiteTale.Server.Domain.Users;

namespace WhiteTale.Server.Features.Identity;

internal static class DependencyInjection
{
	internal static IServiceCollection AddIdentityFeatures(this IServiceCollection services)
	{
		_ = services.AddOptions<IdentityFeatureOptions>()
			.BindConfiguration(IdentityFeatureOptions.SectionName)
			.ValidateDataAnnotations()
			.ValidateOnStart();

		_ = services.AddScoped<ResetPasswordEmailSender>();
		_ = services.AddScoped<ConfirmEmailEmailSender>();

		_ = services.AddOptions<RateLimitOptions>(IdentityRateLimitPolicyNames.ForgotPassword)
			.BindConfiguration($"RateLimiters:{nameof(IdentityRateLimitPolicyNames.ForgotPassword)}")
			.ValidateDataAnnotations()
			.ValidateOnStart();

		_ = services.PostConfigure<RateLimitOptions>(IdentityRateLimitPolicyNames.ForgotPassword, options =>
		{
			options.WindowMilliseconds ??= 30000;
			options.PermitLimit ??= 1;
		});

		_ = services.AddRateLimiter(options =>
		{
			options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

			_ = options.AddPolicy(IdentityRateLimitPolicyNames.ForgotPassword, httpContext =>
			{
				var userManager = httpContext.RequestServices.GetRequiredService<UserManager<User>>();

				var userId = userManager.GetUserId(httpContext.User);
				var partitionKey = userId ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? String.Empty;

				var rateLimit = httpContext.RequestServices
					.GetRequiredService<IOptionsSnapshot<RateLimitOptions>>()
					.Get(IdentityRateLimitPolicyNames.ForgotPassword);
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
		});

		return services;
	}
}
