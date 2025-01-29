using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace WhiteTale.Server.Common.AuthorizationRequirements;

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

		var userManager = httpContext.RequestServices.GetRequiredService<UserManager<User>>();
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
