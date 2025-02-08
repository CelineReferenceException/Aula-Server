using Microsoft.AspNetCore.Identity;

namespace WhiteTale.Server.Common.Identity;

internal static class DependencyInjection
{
	internal static IServiceCollection AddIdentity(this IServiceCollection services)
	{
		_ = services.AddOptions<UserOptions>()
			.BindConfiguration(UserOptions.SectionName)
			.ValidateDataAnnotations()
			.ValidateOnStart();

		_ = services.AddScoped<UserManager>();
		_ = services.AddHostedService<PendingEmailConfirmationsCleanerHostedService>();
		_ = services.AddSingleton<PasswordHasher<User>>();
		_ = services.AddSingleton<TokenProvider>();

		return services;
	}
}
