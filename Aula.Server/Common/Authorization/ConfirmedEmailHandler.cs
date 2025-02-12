using Aula.Server.Common.Identity;
using Aula.Server.Domain.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Aula.Server.Common.Authorization;

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
