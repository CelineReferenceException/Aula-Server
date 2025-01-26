using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace WhiteTale.Server.Common.AuthorizationRequirements;

internal static class PermissionPolicyExtensions
{
	private const String PolicyName = nameof(RequirePermissionAttribute);

	/// <summary>
	///     Enforces the authenticated user to present one of specified permissions.
	/// </summary>
	/// <param name="builder">The endpoint builder.</param>
	/// <param name="permissions">The required permissions. The requirement will succeed if the authenticated user have at least one of these.</param>
	/// <typeparam name="TBuilder">The type of the <paramref name="builder" />.</typeparam>
	/// <returns>The endpoint builder.</returns>
	internal static TBuilder RequirePermission<TBuilder>(this TBuilder builder, params IEnumerable<Permissions> permissions)
		where TBuilder : IEndpointConventionBuilder
	{
		_ = builder
			.RequireAuthorization(PolicyName)
			.WithMetadata(new RequirePermissionAttribute(permissions));
		return builder;
	}

	internal static AuthorizationBuilder AddPermissionPolicy(this AuthorizationBuilder builder)
	{
		_ = builder.AddPolicy(PolicyName, policy => policy.AddRequirements(new PermissionRequirement()));

		builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IAuthorizationHandler, PermissionHandler>());

		return builder;
	}
}
