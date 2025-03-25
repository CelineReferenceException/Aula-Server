using Aula.Server.Core.Api.Gateway;
using Aula.Server.Core.Api.Identity;
using Aula.Server.Core.Api.Messages;
using Aula.Server.Core.Api.Users;

namespace Aula.Server.Core.Api;

internal static class DependencyInjection
{
	internal static IServiceCollection AddApi(this IServiceCollection services)
	{
		_ = services.AddIdentityEndpointEmailSenders();
		_ = services.AddUserPresenceServices();
		_ = services.AddMessageEndpointRateLimiters();
		_ = services.AddGatewayEndpointRateLimiters();

		return services;
	}
}
