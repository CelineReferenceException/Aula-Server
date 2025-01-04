using Microsoft.Extensions.DependencyInjection.Extensions;

namespace WhiteTale.Server.Common.Endpoints;

internal static class EndpointExtensions
{
	internal static IServiceCollection AddEndpoints(this IServiceCollection services)
	{
		var assembly = typeof(IAssemblyMarker).Assembly;

		var descriptors = assembly.DefinedTypes
			.Where(static t => t.IsAssignableTo(typeof(IEndpoint)) && t is { IsInterface: false, IsAbstract: false })
			.Select(static t => ServiceDescriptor.Transient(typeof(IEndpoint), t));

		services.TryAddEnumerable(descriptors);

		return services;
	}

	internal static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder builder)
	{
		ArgumentNullException.ThrowIfNull(builder, nameof(builder));

		var endpoints = builder.ServiceProvider.GetRequiredService<IEnumerable<IEndpoint>>();

		foreach (var endpoint in endpoints)
		{
			endpoint.Build(builder);
		}

		return builder;
	}
}
