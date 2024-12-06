using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WhiteTale.Server;
using WhiteTale.Server.Common.Logging;

long startTimestamp = Stopwatch.GetTimestamp();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IAssemblyMarker).Assembly));

WebApplication app = builder.Build();

await app.StartAsync().ConfigureAwait(false);

TimeSpan elapsedTime = Stopwatch.GetElapsedTime(startTimestamp);
ILogger<Program> logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogMessage($"Now listening on: {string.Join(" - ", app.Urls)}");
logger.LogMessage($"WhiteTale is Ready — It only took {(int)elapsedTime.TotalMilliseconds} milliseconds!");
logger.LogMessage("You can press Ctrl+C to shut down.");

await app.WaitForShutdownAsync().ConfigureAwait(false);
