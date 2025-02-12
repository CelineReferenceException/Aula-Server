using Microsoft.AspNetCore.Authentication;

namespace Aula.Server.Common.Authentication;

internal static class DependencyInjection
{
	internal static IServiceCollection AddApplicationAuthentication(this IServiceCollection services)
	{
		_ = services.AddAuthentication()
			.AddScheme<AuthenticationSchemeOptions, UserAuthenticationHandler>(AuthenticationSchemeNames.BearerToken, _ => { });

		return services;
	}
}
