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

		return services;
	}
}
