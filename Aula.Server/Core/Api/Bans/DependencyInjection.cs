using Microsoft.AspNetCore.Http.Json;

namespace Aula.Server.Core.Api.Bans;

internal static class DependencyInjection
{
	internal static IServiceCollection AddBanApi(this IServiceCollection services)
	{
		_ = services.Configure<JsonOptions>(options => options.SerializerOptions.TypeInfoResolverChain.Add(BanJsonContext.Default));
		return services;
	}
}
