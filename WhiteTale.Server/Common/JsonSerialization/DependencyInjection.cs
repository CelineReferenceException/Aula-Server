using System.Diagnostics;
using System.Text.Json.Serialization;
using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

namespace WhiteTale.Server.Common.JsonSerialization;

internal static class DependencyInjection
{
	internal static IServiceCollection AddJsonSerialization(this IServiceCollection services)
	{
		_ = services.Configure<JsonOptions>(options =>
		{
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
