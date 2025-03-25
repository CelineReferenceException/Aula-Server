using System.Diagnostics;
using Aula.Server.Core.Api;
using MartinCostello.OpenApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;

var startTimestamp = Stopwatch.GetTimestamp();

var builder = WebApplication.CreateBuilder(args);

builder.AddCommon();
builder.Services.AddApi();
builder.Services.AddOpenApi();
builder.Services.AddOpenApiExtensions(static options => options.XmlDocumentationAssemblies.Add(typeof(IAssemblyMarker).Assembly));

var application = builder.Build();

if (!application.Environment.IsDevelopment())
{
	_ = application.UseHttpsRedirection();
}

application.UseCommon();

if (application.Environment.IsDevelopment())
{
	var documentationRoute = application.MapGroup("docs");

	_ = documentationRoute.MapOpenApi();
	_ = documentationRoute.MapScalarApiReference(options =>
	{
		options.WithTitle($"{nameof(Aula)} API {{documentName}}")
			.WithTheme(ScalarTheme.DeepSpace)
			.WithDarkMode(false) // hehehe
			.WithSidebar(true)
			.WithDefaultOpenAllTags(false)
			.WithModels(true)
			.WithDefaultHttpClient(ScalarTarget.JavaScript, ScalarClient.Fetch)
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

	if (e.InnerExceptions.All(innerE => innerE is not OptionsValidationException))
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
