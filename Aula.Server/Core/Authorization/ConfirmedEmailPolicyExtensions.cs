﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Aula.Server.Core.Authorization;

internal static class ConfirmedEmailPolicyExtensions
{
	private const String PolicyName = nameof(ConfirmedEmailRequirement);

	/// <summary>
	///     Require users to have their email confirmed before accessing the endpoint.
	/// </summary>
	/// <param name="builder">The endpoint convention builder.</param>
	/// <returns>The original convention builder parameter.</returns>
	internal static TBuilder RequireConfirmedEmail<TBuilder>(this TBuilder builder)
		where TBuilder : IEndpointConventionBuilder
	{
		return builder.RequireAuthorization(PolicyName);
	}

	/// <summary>
	///     Adds a policy that requires users to have their email confirmed.
	/// </summary>
	/// <param name="builder">The authorization builder.</param>
	/// <returns>The original authorization builder.</returns>
	internal static AuthorizationBuilder AddConfirmedEmailPolicy(this AuthorizationBuilder builder)
	{
		_ = builder.AddPolicy(PolicyName, policy => policy.AddRequirements(new ConfirmedEmailRequirement()));

		builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IAuthorizationHandler, EmailConfirmedHandler>());

		return builder;
	}
}
