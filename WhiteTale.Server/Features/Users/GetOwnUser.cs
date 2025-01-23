using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace WhiteTale.Server.Features.Users;

internal sealed class GetOwnUser : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapGet("api/users/@me", HandleAsync)
			.RequireRateLimiting(CommonRateLimitPolicyNames.Global)
			.RequireAuthorization(IdentityAuthorizationPolicyNames.BearerToken);
	}

	private static async Task<Results<Ok<UserData>, InternalServerError>> HandleAsync(
		HttpContext httpContext,
		[FromServices] UserManager<User> userManager,
		[FromServices] ApplicationDbContext dbContext)
	{
		var userIdClaimValue = userManager.GetUserId(httpContext.User);
		if (!UInt64.TryParse(userIdClaimValue, out var userId))
		{
			return TypedResults.InternalServerError();
		}

		var user = await dbContext.Users
			.AsNoTracking()
			.Where(u => u.Id == userId)
			.Select(u =>
				new UserData
				{
					Id = userId,
					DisplayName = u.DisplayName,
					Description = u.Description,
					CurrentRoomId = u.CurrentRoomId,
					OwnerType = u.OwnerType,
					Presence = u.Presence,
					Permissions = u.Permissions,
				})
			.FirstOrDefaultAsync();
		if (user is null)
		{
			return TypedResults.InternalServerError();
		}

		return TypedResults.Ok(user);
	}
}
