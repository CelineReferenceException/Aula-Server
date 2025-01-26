using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace WhiteTale.Server.Common.AuthorizationRequirements;

internal sealed class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
	protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
	{
		if (context.Resource is not HttpContext httpContext)
		{
			return;
		}

		var endpoint = httpContext.GetEndpoint();
		var requiredPermissions = endpoint?.Metadata.GetMetadata<RequirePermissionAttribute>()?.RequiredPermissions;
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
