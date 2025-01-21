using WhiteTale.Server.Domain.Users;

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
			.Where(x => x.Id == userId)
			.Select(x =>
				new UserData
				{
					Id = userId,
					DisplayName = x.DisplayName,
					Description = x.Description,
					CurrentRoomId = x.CurrentRoomId,
					OwnerType = x.OwnerType,
					Presence = x.Presence
				})
			.FirstOrDefaultAsync();
		if (user is null)
		{
			return TypedResults.InternalServerError();
		}

		return TypedResults.Ok(user);
	}
}
