using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication.BearerToken;
using WhiteTale.Server.Configuration;
using WhiteTale.Server.Domain.Users;

namespace WhiteTale.Server.Common.Identity;

internal static class DependencyInjection
{
	internal static IServiceCollection AddIdentity(this IServiceCollection services, IConfiguration configuration)
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
				var settings = new IdentityOptions();
				configuration.GetSection(IdentityOptions.SectionName).Bind(settings);
				Validator.ValidateObject(settings, new ValidationContext(settings));

				options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyz._";
				options.User.RequireUniqueEmail = true;

				options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(settings.Lockout.LockoutMinutes);
				options.Lockout.MaxFailedAccessAttempts = settings.Lockout.MaximumFailedAccessAttempts;
				options.Lockout.AllowedForNewUsers = settings.Lockout.AllowedForNewUsers;

				options.Password.RequireUppercase = settings.Password.RequireUppercase;
				options.Password.RequireLowercase = settings.Password.RequireLowercase;
				options.Password.RequireDigit = settings.Password.RequireDigit;
				options.Password.RequireNonAlphanumeric = settings.Password.RequireNonAlphanumeric;
				options.Password.RequiredUniqueChars = settings.Password.RequiredUniqueChars;
				options.Password.RequiredLength = settings.Password.RequiredLength;

				options.SignIn.RequireConfirmedAccount = false;
				options.SignIn.RequireConfirmedEmail = settings.SignIn.RequireConfirmedEmail;
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
