using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Aula.Server.Common.CommandLine;

internal static class DependencyInjection
{
	// We hold a reference to the service scope to prevent it from being disposed,
	// ensuring that all commands within the scope remain available and
	// the services injected into them are not being disposed.
	private static IServiceScope? s_serviceScope;

	internal static IServiceCollection AddCommandLine(this IServiceCollection services)
	{
		_ = services.AddSingleton<CommandLineService>();
		_ = services.AddHostedService<CommandLineHostedService>();

		var assembly = typeof(IAssemblyMarker).Assembly;

		var commandTypes = assembly.DefinedTypes
			.Where(t => t.IsAssignableTo(typeof(Command)) && t is { IsInterface: false, IsAbstract: false, })
			.ToArray();

		var abstractDescriptors = commandTypes
			.Select(t => ServiceDescriptor.Transient(typeof(Command), t));

		var concreteDescriptors = commandTypes
			.Select(t => ServiceDescriptor.Transient(t, t));

		services.TryAddEnumerable(abstractDescriptors);
		foreach (var descriptor in concreteDescriptors)
		{
			services.Add(descriptor);
		}

		return services;
	}

	internal static IEndpointRouteBuilder MapCommands(this IEndpointRouteBuilder builder)
	{
		s_serviceScope = builder.ServiceProvider.CreateScope();
		var service = s_serviceScope.ServiceProvider.GetRequiredService<CommandLineService>();
		var commands = s_serviceScope.ServiceProvider.GetRequiredService<IEnumerable<Command>>();

		foreach (var command in commands)
		{
			if (command.GetType().IsAssignableTo(typeof(SubCommand)))
			{
				continue;
			}

			service.AddCommand(command);
		}

		return builder;
	}
}
