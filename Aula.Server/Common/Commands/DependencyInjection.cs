using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Aula.Server.Common.Commands;

internal static class DependencyInjection
{
	// We hold a reference to the service scope to prevent it from being disposed,
	// ensuring that all commands within the scope remain available and
	// the services injected into them are not being disposed.
	private static IServiceScope? s_serviceScope;

	/// <summary>
	///     Registers command-line services and discovers command implementations within the specified assembly.
	/// </summary>
	/// <param name="services">The service collection to configure.</param>
	/// <param name="assemblyType">A type from the assembly containing command implementations.</param>
	/// <returns>The modified <see cref="IServiceCollection" />.</returns>
	internal static IServiceCollection AddCommandLine(this IServiceCollection services, Type assemblyType)
	{
		_ = services.AddSingleton<CommandLine>();
		_ = services.AddHostedService<ConsoleCommandLine>();

		var assembly = assemblyType.Assembly;

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

	/// <summary>
	///     Registers command-line services and discovers command implementations within the specified assembly.
	/// </summary>
	/// <typeparam name="TAssembly">A type within the target assembly.</typeparam>
	/// <param name="services">The service collection to configure.</param>
	/// <returns>The modified <see cref="IServiceCollection" />.</returns>
	internal static IServiceCollection AddCommandLine<TAssembly>(this IServiceCollection services)
	{
		return services.AddCommandLine(typeof(TAssembly));
	}

	internal static TBuilder MapCommands<TBuilder>(this TBuilder builder) where TBuilder : IApplicationBuilder
	{
		s_serviceScope = builder.ApplicationServices.CreateScope();
		var service = s_serviceScope.ServiceProvider.GetRequiredService<CommandLine>();
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
