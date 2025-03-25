using Aula.Server.Core.Authentication;
using Aula.Server.Core.Authorization;
using Aula.Server.Core.Commands;
using Aula.Server.Core.Endpoints;
using Aula.Server.Core.Gateway;
using Aula.Server.Core.Identity;
using Aula.Server.Core.Json;
using Aula.Server.Core.Logging;
using Aula.Server.Core.Mail;
using Aula.Server.Core.Persistence;
using Aula.Server.Core.RateLimiting;
using Aula.Server.Core.Resilience;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Aula.Server.Core;

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
		_ = builder.Services.AddJsonSerialization();

		_ = builder.Services.AddCustomRateLimiter();
		_ = builder.Services.AddMailSender();
		_ = builder.Services.AddSingleton<SnowflakeGenerator>();
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
