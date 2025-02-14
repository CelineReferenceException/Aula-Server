using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Aula.Server.Common.Authorization;

/// <summary>
///     Forbid users who do not have the required permissions specified in the endpoint metadata.
/// </summary>
internal sealed class PermissionsHandler : AuthorizationHandler<PermissionsRequirement>
{
	protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionsRequirement requirement)
	{
		if (context.Resource is not HttpContext httpContext)
		{
			return;
		}

		var endpoint = httpContext.GetEndpoint();
		var requiredPermissions = endpoint?.Metadata.GetMetadata<RequirePermissionsAttribute>()?.RequiredPermissions;
		if (requiredPermissions is null)
		{
			return;
		}

		var userManager = httpContext.RequestServices.GetRequiredService<UserManager>();
		var user = await userManager.GetUserAsync(httpContext.User);
		if (user is null)
		{
			return;
		}

		if (requiredPermissions.Any(permission => user.Permissions.HasFlag(permission)))
		{
			context.Succeed(requirement);
			return;
		}

		context.Fail();
	}
}
