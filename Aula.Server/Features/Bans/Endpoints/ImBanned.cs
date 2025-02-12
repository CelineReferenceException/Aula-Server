using Aula.Server.Common.Endpoints;
using Aula.Server.Common.Identity;
using Aula.Server.Common.Persistence;
using Aula.Server.Common.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Features.Bans.Endpoints;

internal sealed class ImBanned : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapGet("bans/@me", HandleAsync)
			.RequireRateLimiting(RateLimitPolicyNames.Global)
			.RequireAuthenticatedUser()
			.HasApiVersion(1);
	}

	private static async Task<Results<Ok<ImBannedResponse>, InternalServerError>> HandleAsync(
		HttpContext httpContext,
		[FromServices] UserManager userManager,
		[FromServices] ApplicationDbContext dbContext)
	{
		var userId = userManager.GetUserId(httpContext.User);
		if (userId is null)
		{
			return TypedResults.InternalServerError();
		}

		var banned = await dbContext.Bans
			.AsNoTracking()
			.AnyAsync(x => x.TargetId == userId);

		return TypedResults.Ok(new ImBannedResponse
		{
			Banned = banned,
		});
	}
}
