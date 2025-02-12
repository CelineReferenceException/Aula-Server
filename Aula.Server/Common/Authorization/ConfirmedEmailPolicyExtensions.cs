using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Aula.Server.Common.Authorization;

internal static class ConfirmedEmailPolicyExtensions
{
	private const String PolicyName = nameof(ConfirmedEmailRequirement);

	internal static TBuilder RequireConfirmedEmail<TBuilder>(this TBuilder builder)
		where TBuilder : IEndpointConventionBuilder
	{
		return builder.RequireAuthorization(PolicyName);
	}

	internal static AuthorizationBuilder AddConfirmedEmailPolicy(this AuthorizationBuilder builder)
	{
		_ = builder.AddPolicy(PolicyName, policy => policy.AddRequirements(new ConfirmedEmailRequirement()));

		builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IAuthorizationHandler, EmailConfirmedHandler>());

		return builder;
	}
}
