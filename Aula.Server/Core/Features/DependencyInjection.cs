using Aula.Server.Core.Features.Gateway;
using Aula.Server.Core.Features.Identity;
using Aula.Server.Core.Features.Messages;
using Aula.Server.Core.Features.Users;

namespace Aula.Server.Core.Features;

internal static class DependencyInjection
{
	internal static IServiceCollection AddFeatures(this IServiceCollection services)
	{
		_ = services.AddIdentityEndpointEmailSenders();
		_ = services.AddUserPresenceServices();
		_ = services.AddMessageEndpointRateLimiters();
		_ = services.AddGatewayEndpointRateLimiters();

		return services;
	}
}
