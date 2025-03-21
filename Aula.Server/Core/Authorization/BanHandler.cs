using Aula.Server.Core.Identity;
using Aula.Server.Core.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Core.Authorization;

/// <summary>
///     Denies access banned users.
/// </summary>
internal sealed class BanHandler : AuthorizationHandler<BanRequirement>
{
	protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, BanRequirement requirement)
	{
		if (context.Resource is not HttpContext httpContext)
		{
			return;
		}

		var userManager = httpContext.RequestServices.GetRequiredService<UserManager>();
		var user = await userManager.GetUserAsync(httpContext.User);
		if (user is null)
		{
			return;
		}

		var dbContext = httpContext.RequestServices.GetRequiredService<ApplicationDbContext>();

		if (!await dbContext.Bans.AnyAsync(b => b.TargetId == user.Id))
		{
			context.Succeed(requirement);
			return;
		}

		context.Fail();
	}
}
