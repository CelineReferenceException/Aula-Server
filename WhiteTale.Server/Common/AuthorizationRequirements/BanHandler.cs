using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace WhiteTale.Server.Common.AuthorizationRequirements;

internal sealed class BanHandler : AuthorizationHandler<BanRequirement>
{
	protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, BanRequirement requirement)
	{
		if (context.Resource is not HttpContext httpContext)
		{
			return;
		}

		var userManager = httpContext.RequestServices.GetRequiredService<UserManager<User>>();
		var user = await userManager.GetUserAsync(httpContext.User);
		if (user is null)
		{
			return;
		}

		var dbContext = httpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
		var isBanned = await dbContext.Bans
			.AsNoTracking()
			.AnyAsync(b => b.TargetId == user.Id);

		if (!isBanned)
		{
			context.Succeed(requirement);
			return;
		}

		context.Fail();
	}
}
