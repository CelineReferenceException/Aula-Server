using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.Extensions.Hosting;
using WhiteTale.Server.Domain.Users;

namespace WhiteTale.Server.Common;

internal static class DependencyInjection
{
	internal static TBuilder AddCommon<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
	{
		_ = builder.Configuration.AddJsonFile("configuration.json", false);

		_ = builder.Services.AddOptions<ApplicationOptions>()
			.BindConfiguration(ApplicationOptions.SectionName)
			.ValidateDataAnnotations()
			.ValidateOnStart();

		_ = builder.Services.AddAuthentication()
			.AddBearerToken(IdentityConstants.BearerScheme);

		_ = builder.Services.AddAuthorizationBuilder()
			.AddBearerTokenPolicy();

		_ = builder.Services.AddIdentityCore<User>(static options =>
			{
				options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789._";
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

		_ = builder.Services.AddOptions<BearerTokenOptions>(IdentityConstants.BearerScheme)
			.Configure(static options =>
			{
				options.BearerTokenExpiration = TimeSpan.FromDays(1);
				options.RefreshTokenExpiration = TimeSpan.FromDays(14);
			});

		_ = builder.Services.AddCors();
		_ = builder.Services.AddValidatorsFromAssemblyContaining<IAssemblyMarker>(ServiceLifetime.Singleton, includeInternalTypes: true);

		_ = builder.Services.AddRateLimiters();
		_ = builder.Services.AddMailSender();
		_ = builder.Services.AddSingleton<ISnowflakeGenerator, DefaultSnowflakeGenerator>();
		_ = builder.Services.AddDbContext<ApplicationDbContext>();
		_ = builder.Services.AddEndpoints();

		return builder;
	}

	internal static TApp UseCommon<TApp>(this TApp app) where TApp : IApplicationBuilder, IEndpointRouteBuilder
	{
		_ = app.UseCors();
		_ = app.UseRateLimiter();
		_ = app.UseAuthentication();
		_ = app.UseAuthorization();
		_ = app.MapEndpoints();

		return app;
	}
}
