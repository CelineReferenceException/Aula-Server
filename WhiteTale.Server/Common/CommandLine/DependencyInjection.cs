using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace WhiteTale.Server.Common.CommandLine;

internal static class DependencyInjection
{
	internal static IServiceCollection AddCommandLine(this IServiceCollection services)
	{
		_ = services.AddSingleton<CommandLineService>();
		_ = services.AddHostedService<CommandLineHostedService>();

		var assembly = typeof(IAssemblyMarker).Assembly;

		var descriptors = assembly.DefinedTypes
			.Where(static t => t.IsAssignableTo(typeof(Command)) && t is { IsInterface: false, IsAbstract: false, })
			.Select(static t => ServiceDescriptor.Transient(typeof(Command), t));

		services.TryAddEnumerable(descriptors);

		return services;
	}

	internal static IEndpointRouteBuilder MapCommands(this IEndpointRouteBuilder builder)
	{
		var service = builder.ServiceProvider.GetRequiredService<CommandLineService>();
		var commands = builder.ServiceProvider.GetRequiredService<IEnumerable<Command>>();

		foreach (var command in commands)
		{
			service.AddCommand(command);
		}

		return builder;
	}
}
