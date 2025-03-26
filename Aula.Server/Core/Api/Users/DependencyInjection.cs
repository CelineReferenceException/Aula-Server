using Microsoft.AspNetCore.Http.Json;

namespace Aula.Server.Core.Api.Users;

internal static class DependencyInjection
{
	internal static IServiceCollection AddUserApi(this IServiceCollection services)
	{
		_ = services.Configure<JsonOptions>(options => options.SerializerOptions.TypeInfoResolverChain.Add(UserJsonContext.Default));
		_ = services.AddHostedService<ResetPresencesService>();
		return services;
	}
}
