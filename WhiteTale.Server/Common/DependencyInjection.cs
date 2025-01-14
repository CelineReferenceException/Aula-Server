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

		_ = builder.Services.AddIdentity(builder.Configuration);
		_ = builder.Services.AddAuthorizationRequirements();

		_ = builder.Services.AddCors();
		_ = builder.Services.AddValidatorsFromAssemblyContaining<IAssemblyMarker>(ServiceLifetime.Singleton, includeInternalTypes: true);

		_ = builder.Services.AddRateLimiters();
		_ = builder.Services.AddMailSender();
		_ = builder.Services.AddSingleton<ISnowflakeGenerator, DefaultSnowflakeGenerator>();
		_ = builder.Services.AddPersistence(builder.Configuration);
		_ = builder.Services.AddEndpoints();
		_ = builder.Logging.ClearProviders();
		_ = builder.Logging.AddLogging();

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
