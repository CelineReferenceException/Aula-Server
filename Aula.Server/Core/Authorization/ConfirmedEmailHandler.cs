using Aula.Server.Core.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Aula.Server.Core.Authorization;

/// <summary>
///     Require users to have their email confirmed.
/// </summary>
internal sealed class EmailConfirmedHandler : AuthorizationHandler<ConfirmedEmailRequirement>
{
	protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ConfirmedEmailRequirement requirement)
	{
		if (context.Resource is not HttpContext httpContext)
		{
			return;
		}

		var userManager = httpContext.RequestServices.GetRequiredService<UserManager>();
		var user = await userManager.GetUserAsync(httpContext.User);
		if (user?.Type is not UserType.Standard)
		{
			return;
		}

		if (!userManager.Options.SignIn.RequireConfirmedEmail ||
		    user.EmailConfirmed)
		{
			context.Succeed(requirement);
		}
	}
}
