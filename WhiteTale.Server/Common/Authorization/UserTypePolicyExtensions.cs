using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace WhiteTale.Server.Common.Authorization;

internal static class UserTypePolicyExtensions
{
	private const String PolicyName = nameof(UserTypeRequirement);

	internal static TBuilder RequireUserType<TBuilder>(this TBuilder builder, params IEnumerable<UserType> authorizedTypes)
		where TBuilder : IEndpointConventionBuilder
	{
		_ = builder
			.RequireAuthorization(PolicyName)
			.WithMetadata(new RequireUserTypeAttribute(authorizedTypes.ToArray()));
		return builder;
	}

	internal static AuthorizationBuilder AddUserTypePolicy(this AuthorizationBuilder builder)
	{
		_ = builder.AddPolicy(PolicyName, policy => policy.AddRequirements(new UserTypeRequirement()));

		builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IAuthorizationHandler, UserTypeHandler>());

		return builder;
	}
}
