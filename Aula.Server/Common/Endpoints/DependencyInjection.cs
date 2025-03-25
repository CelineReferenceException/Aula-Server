using Asp.Versioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Aula.Server.Common.Endpoints;

internal static class DependencyInjection
{
	internal static IServiceCollection AddEndpoints(this IServiceCollection services, Type assemblyType)
	{
		var descriptors = assemblyType.Assembly.DefinedTypes
			.Where(static t => t.IsAssignableTo(typeof(IEndpoint)) && t is { IsInterface: false, IsAbstract: false, })
			.Select(static t => ServiceDescriptor.Transient(typeof(IEndpoint), t));

		services.TryAddEnumerable(descriptors);

		_ = services.AddApiVersioning(options =>
		{
			options.DefaultApiVersion = new ApiVersion(1);
			options.ApiVersionReader = new UrlSegmentApiVersionReader();
			options.UnsupportedApiVersionStatusCode = StatusCodes.Status404NotFound;
		});

		return services;
	}

	internal static IServiceCollection AddEndpoints<TAssemblyType>(this IServiceCollection services)
	{
		return AddEndpoints(services, typeof(TAssemblyType));
	}

	internal static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder builder)
	{
		ArgumentNullException.ThrowIfNull(builder, nameof(builder));

		var apiVersionSet = builder.NewApiVersionSet()
			.HasApiVersion(new ApiVersion(1))
			.Build();

		var apiGroup = builder.MapGroup("api/v{apiVersion:apiVersion}").WithApiVersionSet(apiVersionSet);

		var endpoints = builder.ServiceProvider.GetRequiredService<IEnumerable<IEndpoint>>();

		foreach (var endpoint in endpoints)
		{
			endpoint.Build(apiGroup);
		}

		return builder;
	}
}
