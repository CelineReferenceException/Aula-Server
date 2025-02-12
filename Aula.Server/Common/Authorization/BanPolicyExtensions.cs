using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Aula.Server.Common.Authorization;

internal static class BanPolicyExtensions
{
	private const String PolicyName = nameof(BanRequirement);

	internal static TBuilder DenyBannedUsers<TBuilder>(this TBuilder builder)
		where TBuilder : IEndpointConventionBuilder
	{
		return builder.RequireAuthorization(PolicyName);
	}

	internal static AuthorizationBuilder AddBanPolicy(this AuthorizationBuilder builder)
	{
		_ = builder.AddPolicy(PolicyName, policy => policy.AddRequirements(new BanRequirement()));

		builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IAuthorizationHandler, BanHandler>());

		return builder;
	}
}
