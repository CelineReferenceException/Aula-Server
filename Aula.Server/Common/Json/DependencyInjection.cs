using System.Diagnostics;
using System.Reflection;
using System.Text.Json.Serialization;
using Aula.Server.Domain.Users;
using Microsoft.AspNetCore.Http.Json;

namespace Aula.Server.Common.Json;

internal static class DependencyInjection
{
	internal static IServiceCollection AddJson<TAssemblyType>(this IServiceCollection services)
	{
		return services.AddJson(typeof(TAssemblyType).Assembly);
	}

	internal static IServiceCollection AddJson(this IServiceCollection services, Assembly assemblyType)
	{
		_ = services.Configure<JsonOptions>(options =>
		{
			options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

			var converters = assemblyType.DefinedTypes
				.Where(x => x.BaseType is not null && !x.IsGenericType && x.IsAssignableTo(typeof(JsonConverter)));

			foreach (var converter in converters)
			{
				var instance = Activator.CreateInstance(converter) as JsonConverter ?? throw new UnreachableException();
				options.SerializerOptions.Converters.Add(instance);
			}

			options.SerializerOptions.Converters.Add(new UInt64EnumToStringConverter<Permissions>());
			options.SerializerOptions.TypeInfoResolverChain.Add(CommonJsonContext.Default);
		});

		return services;
	}
}
