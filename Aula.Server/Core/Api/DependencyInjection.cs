using Aula.Server.Core.Api.Bans;
using Aula.Server.Core.Api.Bots;
using Aula.Server.Core.Api.Gateway;
using Aula.Server.Core.Api.Identity;
using Aula.Server.Core.Api.Messages;
using Aula.Server.Core.Api.Rooms;
using Aula.Server.Core.Api.Users;
using Microsoft.AspNetCore.Http.Json;

namespace Aula.Server.Core.Api;

internal static class DependencyInjection
{
	internal static IServiceCollection AddApi(this IServiceCollection services)
	{
		_ = services.AddBanApi();
		_ = services.AddBotApi();
		_ = services.AddGatewayApi();
		_ = services.AddIdentityApi();
		_ = services.AddMessageApi();
		_ = services.AddRoomApi();
		_ = services.AddUserApi();

		_ = services.Configure<JsonOptions>(options => options.SerializerOptions.TypeInfoResolverChain.Add(ApiJsonContext.Default));

		return services;
	}
}
