using System.Diagnostics;
using Aula.Server.Common.Authentication;
using Aula.Server.Common.Authorization;
using Aula.Server.Common.Identity;
using Aula.Server.Common.Json;
using Aula.Server.Common.Logging;
using Aula.Server.Common.Mail;
using Aula.Server.Common.Persistence;
using Aula.Server.Common.Resilience;
using Aula.Server.Core.Api;
using FluentValidation;
using MartinCostello.OpenApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;

var startTimestamp = Stopwatch.GetTimestamp();

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("configuration.json", false, true);

builder.Services.AddOptions<ApplicationOptions>()
	.BindConfiguration(ApplicationOptions.SectionName)
	.ValidateDataAnnotations()
	.ValidateOnStart();

builder.Services.AddCors(options =>
{
	options.AddDefaultPolicy(policy =>
	{
		_ = policy.AllowAnyOrigin()
			.AllowAnyHeader()
			.AllowAnyMethod();
	});
});

builder.Services.AddOpenApi();
builder.Services.AddOpenApiExtensions(static options => options.XmlDocumentationAssemblies.Add(typeof(IAssemblyMarker).Assembly));
builder.Services.AddValidatorsFromAssemblyContaining<IAssemblyMarker>(ServiceLifetime.Singleton, includeInternalTypes: true);
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<IAssemblyMarker>());
builder.Services.AddSingleton<SnowflakeGenerator>();
builder.Services.AddSingleton<TokenProvider>();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddResilience();
builder.Services.AddJson<IAssemblyMarker>();
builder.Services.AddCustomRateLimiter();
builder.Services.AddMailSender();
builder.Services.AddIdentity();
builder.Services.AddCustomAuthentication();
builder.Services.AddCustomAuthorization();
builder.Services.AddEndpoints<IAssemblyMarker>();
builder.Services.AddGateway();
builder.Services.AddCommands<IAssemblyMarker>();
builder.Services.AddApi();

builder.Logging.ClearProviders();
builder.Logging.AddLogging();

var application = builder.Build();

application.MapCommands();

if (!application.Environment.IsDevelopment())
{
	_ = application.UseHttpsRedirection();
}

application.UseCors();
application.UseWebSockets();
application.UseWebSocketHeaderParsing();
application.UseAuthentication();
application.UseCustomRateLimiting();
application.UseAuthorization();
application.MapEndpoints();

if (application.Environment.IsDevelopment())
{
	var documentationRoute = application.MapGroup("docs");

	_ = documentationRoute.MapOpenApi();
	_ = documentationRoute.MapScalarApiReference(options =>
	{
		options.WithTitle($"{nameof(Aula)} API {{documentName}}")
			.WithTheme(ScalarTheme.Alternate)
			.WithDarkMode(false) // hehehe
			.WithSidebar(true)
			.WithDefaultOpenAllTags(false)
			.WithModels(true)
			.WithDefaultHttpClient(ScalarTarget.Node, ScalarClient.Fetch)
			.WithEndpointPrefix("/scalar/{documentName}")
			.OpenApiRoutePattern = "/docs/openapi/{documentName}.json";
	});
}

try
{
	await application.StartAsync();
}
catch (AggregateException e)
{
	foreach (var innerException in e.InnerExceptions)
	{
		if (innerException is not OptionsValidationException validationException)
		{
			continue;
		}

		foreach (var failure in validationException.Failures)
		{
			Console.WriteLine(failure);
		}
	}

	if (e.InnerExceptions.Any(innerE => innerE is not OptionsValidationException))
	{
		throw;
	}
}

var elapsedTime = Stopwatch.GetElapsedTime(startTimestamp);
var logger = application.Services.GetRequiredService<ILogger<Program>>();

logger.LogStartupMessage($"Now listening on: {String.Join(" - ", application.Urls)}");
logger.LogStartupMessage($"{nameof(Aula)} is Ready — It only took {(Int32)elapsedTime.TotalMilliseconds} milliseconds!");
logger.LogStartupMessage("Type 'help' to see a list of available commands.");
logger.LogStartupMessage("You can press Ctrl+C to shut down.");

await application.WaitForShutdownAsync();
