using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace WhiteTale.Server.Features.Bans;

internal sealed class ImBanned : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapGet("bans/@me", HandleAsync)
			.RequireRateLimiting(CommonRateLimitPolicyNames.Global)
			.RequireAuthorization(IdentityAuthorizationPolicyNames.BearerToken)
			.HasApiVersion(1);
	}

	private static async Task<Results<Ok<ImBannedData>, InternalServerError>> HandleAsync(
		HttpContext httpContext,
		[FromServices] UserManager<User> userManager,
		[FromServices] ApplicationDbContext dbContext)
	{
		var userIdClaimValue = userManager.GetUserId(httpContext.User);
		if (!UInt64.TryParse(userIdClaimValue, out var userId))
		{
			return TypedResults.InternalServerError();
		}

		var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
		var isBanned = false;
		if (ipAddress is not null)
		{
			isBanned = await dbContext.Bans
				.AsTracking()
				.AnyAsync(x => x.TargetId == userId || x.IpAddress == ipAddress);
		}
		else
		{
			isBanned = await dbContext.Bans
				.AsTracking()
				.AnyAsync(x => x.TargetId == userId);
		}

		return TypedResults.Ok(new ImBannedData
		{
			IsBanned = isBanned,
		});
	}
}
