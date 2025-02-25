using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Aula.Server.Common;

internal static class DependencyInjection
{
	internal static TBuilder AddCommon<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
	{
		_ = builder.Configuration.AddJsonFile("configuration.json", false);

		_ = builder.Services.AddOptions<ApplicationOptions>()
			.BindConfiguration(ApplicationOptions.SectionName)
			.ValidateDataAnnotations()
			.ValidateOnStart();

		_ = builder.Services.AddIdentity();
		_ = builder.Services.AddApplicationAuthentication();
		_ = builder.Services.AddApplicationAuthorization();

		_ = builder.Services.AddCors();
		_ = builder.Services.AddValidatorsFromAssemblyContaining<IAssemblyMarker>(ServiceLifetime.Singleton, includeInternalTypes: true);
		_ = builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<IAssemblyMarker>());
		_ = builder.Services.AddJsonSerialization();

		_ = builder.Services.AddRateLimiters();
		_ = builder.Services.AddMailSender();
		_ = builder.Services.AddSingleton<SnowflakeGenerator>();
		_ = builder.Services.AddPersistence(builder.Configuration);
		_ = builder.Services.AddResilience();
		_ = builder.Services.AddEndpoints();
		_ = builder.Services.AddGateway();

		_ = builder.Logging.ClearProviders();
		_ = builder.Logging.AddLogging();
		_ = builder.Services.AddCommandLine<IAssemblyMarker>();

		return builder;
	}

	internal static TApp UseCommon<TApp>(this TApp app) where TApp : IApplicationBuilder, IEndpointRouteBuilder
	{
		_ = app.UseCors();
		_ = app.UseRateLimiter();
		_ = app.UseAuthentication();
		_ = app.UseAuthorization();
		_ = app.UseWebSockets();
		_ = app.MapEndpoints();
		_ = app.MapCommands();

		return app;
	}
}
