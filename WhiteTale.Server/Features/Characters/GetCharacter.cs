using WhiteTale.Server.Common.RateLimiting;

namespace WhiteTale.Server.Features.Characters;

internal sealed class GetCharacter : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapGet("api/characters/{characterId}", HandleAsync)
			.RequireRateLimiting(RateLimitPolicyNames.Global)
			.RequireAuthorization(AuthorizationPolicyNames.BearerToken);
	}

	private static async Task<Results<Ok<CharacterData>, NotFound>> HandleAsync(
		[FromRoute] UInt64 characterId,
		[FromServices] ApplicationDbContext dbContext)
	{
		var character = await dbContext.Characters
			.AsNoTracking()
			.Where(character => character.Id == characterId)
			.Select(character =>
				new CharacterData
				{
					Id = characterId,
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
			return TypedResults.NotFound();
		}

		return TypedResults.Ok(character);
	}
}
