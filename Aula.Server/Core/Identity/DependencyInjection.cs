using Microsoft.AspNetCore.Identity;

namespace Aula.Server.Core.Identity;

internal static class DependencyInjection
{
	internal static IServiceCollection AddIdentity(this IServiceCollection services)
	{
		_ = services.AddOptions<IdentityOptions>()
			.BindConfiguration(IdentityOptions.SectionName)
			.ValidateDataAnnotations()
			.ValidateOnStart();

		_ = services.AddScoped<UserManager>();
		_ = services.AddHostedService<PendingEmailConfirmationsCleanerService>();
		_ = services.AddHostedService<PendingPasswordResetsCleanerService>();
		_ = services.AddSingleton<PasswordHasher<User>>();
		_ = services.AddSingleton<TokenProvider>();

		return services;
	}
}
