using Microsoft.AspNetCore.Authentication;

namespace WhiteTale.Server.Common.Authentication;

internal static class DependencyInjection
{
	internal static IServiceCollection AddApplicationAuthentication(this IServiceCollection services)
	{
		_ = services.AddAuthentication()
			.AddScheme<AuthenticationSchemeOptions, UserAuthenticationHandler>(AuthenticationSchemeNames.BearerToken, options => { });

		return services;
	}
}
