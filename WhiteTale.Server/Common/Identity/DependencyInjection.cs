using Microsoft.AspNetCore.Authentication.BearerToken;
using WhiteTale.Server.Domain.Users;

namespace WhiteTale.Server.Common.Identity;

internal static class DependencyInjection
{
	internal static IServiceCollection AddIdentity(this IServiceCollection services)
	{
		_ = services.AddAuthentication()
			.AddBearerToken(IdentityConstants.BearerScheme);

		_ = services.AddAuthorizationBuilder()
			.AddPolicy(IdentityAuthorizationPolicyNames.BearerToken, policy =>
			{
				_ = policy.RequireAuthenticatedUser();
				_ = policy.AddAuthenticationSchemes(IdentityConstants.BearerScheme);
			});

		_ = services.AddIdentityCore<User>(options =>
			{
				options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyz._";
				options.User.RequireUniqueEmail = true;

				options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
				options.Lockout.MaxFailedAccessAttempts = 10;
				options.Lockout.AllowedForNewUsers = true;

				options.Password.RequireUppercase = true;
				options.Password.RequireLowercase = true;
				options.Password.RequireDigit = true;
				options.Password.RequireNonAlphanumeric = true;
				options.Password.RequiredUniqueChars = 0;
				options.Password.RequiredLength = 8;

				options.SignIn.RequireConfirmedAccount = false;
				options.SignIn.RequireConfirmedEmail = true;
				options.SignIn.RequireConfirmedPhoneNumber = false;
			})
			.AddEntityFrameworkStores<ApplicationDbContext>()
			.AddSignInManager()
			.AddDefaultTokenProviders();

		_ = services.AddOptions<BearerTokenOptions>(IdentityConstants.BearerScheme)
			.Configure(static options =>
			{
				options.BearerTokenExpiration = TimeSpan.FromDays(1);
				options.RefreshTokenExpiration = TimeSpan.FromDays(14);
			});

		return services;
	}
}
