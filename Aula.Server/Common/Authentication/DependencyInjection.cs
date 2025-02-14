using Microsoft.AspNetCore.Authentication;

namespace Aula.Server.Common.Authentication;

internal static class DependencyInjection
{
	/// <summary>
	///     Adds the authentication services used by the application.
	/// </summary>
	/// <param name="services">The <see cref="IServiceCollection" /> to add the services to.</param>
	/// <returns>A reference to this instance after the operation has completed.</returns>
	internal static IServiceCollection AddApplicationAuthentication(this IServiceCollection services)
	{
		_ = services.AddAuthentication()
			.AddScheme<AuthenticationSchemeOptions, UserAuthenticationHandler>(AuthenticationSchemeNames.BearerToken, _ => { });

		return services;
	}
}
