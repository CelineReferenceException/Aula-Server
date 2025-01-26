namespace WhiteTale.Server.Common.CommandLine;

internal static class DependencyInjection
{
	internal static IServiceCollection AddCommandLine(this IServiceCollection services)
	{
		_ = services.AddSingleton<CommandLineService>();
		_ = services.AddHostedService<CommandLineHostedService>();

		return services;
	}
}
