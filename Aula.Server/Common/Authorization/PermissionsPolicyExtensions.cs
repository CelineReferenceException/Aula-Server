using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Aula.Server.Common.Authorization;

internal static class PermissionsPolicyExtensions
{
	private const String PolicyName = nameof(PermissionsRequirement);

	/// <summary>
	///     Enforces the authenticated user to present one of specified permissions.
	/// </summary>
	/// <param name="builder">The endpoint builder.</param>
	/// <param name="permissions">
	///     The required permissions. The requirement will succeed if the authenticated user have
	///     at least one of these, or <see cref="Permissions.Administrator" />.
	/// </param>
	/// <typeparam name="TBuilder">The type of the <paramref name="builder" />.</typeparam>
	/// <returns>The endpoint builder.</returns>
	internal static TBuilder RequirePermissions<TBuilder>(this TBuilder builder, params IEnumerable<Permissions> permissions)
		where TBuilder : IEndpointConventionBuilder
	{
		return builder
			.RequireAuthorization(PolicyName)
			.WithMetadata(new RequirePermissionsAttribute([Permissions.Administrator, ..permissions,]));
	}

	internal static AuthorizationBuilder AddPermissionsPolicy(this AuthorizationBuilder builder)
	{
		_ = builder.AddPolicy(PolicyName, policy => policy.AddRequirements(new PermissionsRequirement()));

		builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IAuthorizationHandler, PermissionsHandler>());

		return builder;
	}
}
