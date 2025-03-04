using Aula.Server.Features.Gateway.Endpoints;
using Aula.Server.Features.Identity.Endpoints;
using Aula.Server.Features.Messages.Endpoints;
using Aula.Server.Features.Users;

namespace Aula.Server.Features;

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
