using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Common.Authorization;

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
		var banned = await dbContext.Bans
			.AsNoTracking()
			.AnyAsync(b => b.TargetId == user.Id);

		if (!banned)
		{
			context.Succeed(requirement);
			return;
		}

		context.Fail();
	}
}
