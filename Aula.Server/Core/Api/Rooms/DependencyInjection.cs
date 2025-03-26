using Microsoft.AspNetCore.Http.Json;

namespace Aula.Server.Core.Api.Rooms;

internal static class DependencyInjection
{
	internal static IServiceCollection AddRoomApi(this IServiceCollection services)
	{
		_ = services.Configure<JsonOptions>(options => options.SerializerOptions.TypeInfoResolverChain.Add(RoomJsonContext.Default));
		return services;
	}
}
