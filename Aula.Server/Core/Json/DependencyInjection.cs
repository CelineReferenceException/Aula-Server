using System.Diagnostics;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;

namespace Aula.Server.Core.Json;

internal static class DependencyInjection
{
	internal static IServiceCollection AddJsonSerialization(this IServiceCollection services)
	{
		_ = services.Configure<JsonOptions>(options =>
		{
			options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

			var converters = typeof(IAssemblyMarker).Assembly.DefinedTypes
				.Where(x => x.BaseType is not null && !x.IsGenericType && x.IsAssignableTo(typeof(JsonConverter)));

			foreach (var converter in converters)
			{
				var instance = Activator.CreateInstance(converter) as JsonConverter ?? throw new UnreachableException();
				options.SerializerOptions.Converters.Add(instance);
			}
		});

		return services;
	}
}
