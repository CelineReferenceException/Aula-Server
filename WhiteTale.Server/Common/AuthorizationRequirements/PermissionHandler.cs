using Microsoft.AspNetCore.Authorization;
using WhiteTale.Server.Domain.Users;

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
		var user = await userManager.GetUserAsync(httpContext.User).ConfigureAwait(false);
		if (user is null)
		{
			return;
		}

		if (user.Permissions.HasFlag(Permissions.Administrator)
		    || requiredPermissions.Any(permission => user.Permissions.HasFlag(permission)))
		{
			context.Succeed(requirement);
			return;
		}

		context.Fail();
	}
}
