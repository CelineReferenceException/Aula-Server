using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Features.Bans.Endpoints;

internal sealed class UnbanUser : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapDelete("bans/users/{targetId}", HandleAsync)
			.RequireRateLimiting(RateLimitPolicyNames.Global)
			.RequireAuthenticatedUser()
			.RequirePermissions(Permissions.BanUsers)
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<NoContent, ProblemHttpResult, InternalServerError>> HandleAsync(
		[FromRoute] UInt64 targetId,
		[FromServices] ApplicationDbContext dbContext)
	{
		var ban = await dbContext.Bans
			.AsTracking()
			.Where(x => x.TargetId == targetId)
			.FirstOrDefaultAsync();
		if (ban is null)
		{
			return TypedResults.NoContent();
		}

		ban.Remove();
		_ = dbContext.Bans.Remove(ban);

		try
		{
			_ = await dbContext.SaveChangesAsync();
		}
		catch (DbUpdateConcurrencyException)
		{
			return TypedResults.InternalServerError();
		}

		return TypedResults.NoContent();
	}
}
