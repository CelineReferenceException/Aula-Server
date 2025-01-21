namespace WhiteTale.Server.Features.Users;

internal sealed class GetUser : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapGet("api/users/{userId}", HandleAsync)
			.RequireRateLimiting(CommonRateLimitPolicyNames.Global)
			.RequireAuthorization(IdentityAuthorizationPolicyNames.BearerToken);
	}

	private static async Task<Results<Ok<UserData>, NotFound>> HandleAsync(
		[FromRoute] UInt64 userId,
		[FromServices] ApplicationDbContext dbContext)
	{
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
			return TypedResults.NotFound();
		}

		return TypedResults.Ok(user);
	}
}
