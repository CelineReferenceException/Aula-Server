using Aula.Server.Core.Authorization;
using Aula.Server.Core.Endpoints;
using Aula.Server.Core.Identity;
using Aula.Server.Core.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Core.Features.Bans.Endpoints;

internal sealed class GetCurrentUserBanStatus : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapGet("bans/@me", HandleAsync)
			.RequireAuthenticatedUser()
			.HasApiVersion(1);
	}

	private static async Task<Results<Ok<GetCurrentUserBanStatusResponse>, InternalServerError>> HandleAsync(
		HttpContext httpContext,
		[FromServices] UserManager userManager,
		[FromServices] ApplicationDbContext dbContext)
	{
		var userId = userManager.GetUserId(httpContext.User);
		if (userId is null)
		{
			return TypedResults.InternalServerError();
		}

		return TypedResults.Ok(new GetCurrentUserBanStatusResponse
		{
			Banned = await dbContext.Bans.AnyAsync(x => x.TargetId == userId),
		});
	}
}
