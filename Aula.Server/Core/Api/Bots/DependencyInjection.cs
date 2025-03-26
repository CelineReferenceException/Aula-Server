using Microsoft.AspNetCore.Http.Json;

namespace Aula.Server.Core.Api.Bots;

internal static class DependencyInjection
{
	internal static IServiceCollection AddBotApi(this IServiceCollection services)
	{
		_ = services.Configure<JsonOptions>(options => options.SerializerOptions.TypeInfoResolverChain.Add(BotJsonContext.Default));
		return services;
	}
}
