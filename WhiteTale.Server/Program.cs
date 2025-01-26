using System.Diagnostics;
using MartinCostello.OpenApi;
using Microsoft.AspNetCore.Builder;
using Scalar.AspNetCore;
using WhiteTale.Server.Features;

var startTimestamp = Stopwatch.GetTimestamp();

var builder = WebApplication.CreateBuilder(args);

builder.AddCommon();
builder.Services.AddFeatures();

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
		options.WithTitle($"{nameof(WhiteTale)} API {{documentName}}")
			.WithTheme(ScalarTheme.DeepSpace)
			.WithDarkMode(false)
			.WithSidebar(true)
			.WithDefaultOpenAllTags(false)
			.WithModels(true)
			.WithDefaultHttpClient(ScalarTarget.JavaScript, ScalarClient.Fetch)
			.WithEndpointPrefix("/api-reference/{documentName}")
			.OpenApiRoutePattern = "/docs/openapi/{documentName}.json";
	});
}

await application.StartAsync();

var elapsedTime = Stopwatch.GetElapsedTime(startTimestamp);
var logger = application.Services.GetRequiredService<ILogger<Program>>();

logger.Inform($"Now listening on: {String.Join(" - ", application.Urls)}");
logger.Inform($"{nameof(WhiteTale)} is Ready — It only took {(Int32)elapsedTime.TotalMilliseconds} milliseconds!");
logger.Inform("You can press Ctrl+C to shut down.");

await application.WaitForShutdownAsync();
