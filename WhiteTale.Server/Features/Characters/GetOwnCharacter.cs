using System.Security.Claims;
using WhiteTale.Server.Common.RateLimiting;
using WhiteTale.Server.Domain.Users;

namespace WhiteTale.Server.Features.Characters;

internal sealed class GetOwnCharacter : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapGet("api/characters/@me", HandleAsync)
			.RequireRateLimiting(RateLimitPolicyNames.Global)
			.RequireAuthorization(AuthorizationPolicyNames.BearerToken);
	}

	private static async Task<Results<Ok<CharacterData>, InternalServerError>> HandleAsync(
		[FromServices] HttpContext httpContext,
		[FromServices] UserManager<User> userManager,
		[FromServices] ApplicationDbContext dbContext)
	{
		var userIdClaimValue = userManager.GetUserId(httpContext.User);
		if (!UInt64.TryParse(userIdClaimValue, out var userId))
		{
			return TypedResults.InternalServerError();
		}

		var character = await dbContext.Characters
			.AsNoTracking()
			.Where(character => character.Id == userId)
			.Select(character =>
				new CharacterData
				{
					Id = userId,
					DisplayName = character.DisplayName,
					Description = character.Description,
					CurrentRoomId = character.CurrentRoomId,
					OwnerType = character.OwnerType,
					Presence = character.Presence,
					Permissions = character.Permissions
				})
			.SingleOrDefaultAsync()
			.ConfigureAwait(false);
		if (character is null)
		{
			return TypedResults.InternalServerError();
		}

		return TypedResults.Ok(character);
	}
}
