using Aula.Server.Common.Authentication;
using Aula.Server.Common.Authorization;
using Aula.Server.Common.Endpoints;
using Aula.Server.Common.Gateway;
using Aula.Server.Common.Identity;
using Aula.Server.Common.Json;
using Aula.Server.Common.Logging;
using Aula.Server.Common.Mail;
using Aula.Server.Common.Persistence;
using Aula.Server.Common.RateLimiting;
using Aula.Server.Common.Resilience;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Aula.Server.Common;

internal static class DependencyInjection
{
	internal static TBuilder AddCommon<TBuilder>(this TBuilder builder)
		where TBuilder : IHostApplicationBuilder
	{
		_ = builder.Configuration.AddJsonFile("configuration.json", false, true);

		_ = builder.Services.AddOptions<ApplicationOptions>()
			.BindConfiguration(ApplicationOptions.SectionName)
			.ValidateDataAnnotations()
			.ValidateOnStart();

		_ = builder.Services.AddIdentity();
		_ = builder.Services.AddCustomAuthentication();
		_ = builder.Services.AddCustomAuthorization();

		_ = builder.Services.AddCors(options =>
		{
			options.AddDefaultPolicy(policy =>
			{
				_ = policy.AllowAnyOrigin()
					.AllowAnyHeader()
					.AllowAnyMethod();
			});
		});
		_ = builder.Services.AddValidatorsFromAssemblyContaining<IAssemblyMarker>(ServiceLifetime.Singleton, includeInternalTypes: true);
		_ = builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<IAssemblyMarker>());
		_ = builder.Services.AddJson<IAssemblyMarker>();

		_ = builder.Services.AddCustomRateLimiter();
		_ = builder.Services.AddMailSender();
		_ = builder.Services.AddSingleton<SnowflakeGenerator>();
		_ = builder.Services.AddSingleton<TokenProvider>();
		_ = builder.Services.AddPersistence(builder.Configuration);
		_ = builder.Services.AddResilience();
		_ = builder.Services.AddEndpoints<IAssemblyMarker>();
		_ = builder.Services.AddGateway();

		_ = builder.Logging.ClearProviders();
		_ = builder.Logging.AddLogging();
		_ = builder.Services.AddCommandLine<IAssemblyMarker>();

		return builder;
	}

	internal static TApp UseCommon<TApp>(this TApp app)
		where TApp : IApplicationBuilder, IEndpointRouteBuilder
	{
		_ = app.UseCors();
		_ = app.UseWebSockets();
		_ = app.UseWebSocketHeaderParsing();
		_ = app.UseAuthentication();
		_ = app.UseCustomRateLimiting();
		_ = app.UseAuthorization();
		_ = app.MapEndpoints();
		_ = app.MapCommands();

		return app;
	}
}
