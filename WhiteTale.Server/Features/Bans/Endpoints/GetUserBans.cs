using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace WhiteTale.Server.Features.Bans.Endpoints;

internal sealed class GetUserBans : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapGet("bans/users", HandleAsync)
			.RequireRateLimiting(RateLimitPolicyNames.Global)
			.RequireAuthorization(IdentityAuthorizationPolicyNames.BearerToken)
			.RequirePermissions(Permissions.BanUsers)
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Ok<List<BanData>>> HandleAsync([FromServices] ApplicationDbContext dbContext)
	{
		var bans = await dbContext.Bans
			.Where(b => b.Type == BanType.Id)
			.Select(b => new BanData
			{
				Type = b.Type,
				ExecutorId = b.ExecutorId,
				Reason = b.Reason,
				TargetId = b.TargetId,
				CreationTime = b.CreationTime,
			})
			.ToListAsync();

		return TypedResults.Ok(bans);
	}
}
