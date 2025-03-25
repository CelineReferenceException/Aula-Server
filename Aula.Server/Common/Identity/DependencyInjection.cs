using Aula.Server.Domain.Users;
using Microsoft.AspNetCore.Identity;

namespace Aula.Server.Common.Identity;

internal static class DependencyInjection
{
	internal static IServiceCollection AddIdentity(this IServiceCollection services)
	{
		_ = services.AddOptions<IdentityOptions>()
			.BindConfiguration(IdentityOptions.SectionName)
			.ValidateDataAnnotations()
			.ValidateOnStart();

		services.TryAddScoped<UserManager>();
		services.TryAddSingleton<PasswordHasher<User>>();
		_ = services.AddHostedService<PendingEmailConfirmationsCleanerService>();
		_ = services.AddHostedService<PendingPasswordResetsCleanerService>();

		_ = services.Configure<PasswordHasherOptions>(static options =>
		{
			options.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3;
			options.IterationCount = 100000;
		});

		return services;
	}
}
