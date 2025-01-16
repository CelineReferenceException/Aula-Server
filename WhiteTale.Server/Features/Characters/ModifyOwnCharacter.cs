using WhiteTale.Server.Domain.Users;

namespace WhiteTale.Server.Features.Characters;

internal sealed class ModifyOwnCharacter : IEndpoint
{
	public void Build(IEndpointRouteBuilder builder)
	{
		_ = builder.MapPatch("api/characters/@me", HandleAsync)
			.RequireRateLimiting(RateLimitPolicyNames.Global)
			.RequireAuthorization(IdentityAuthorizationPolicyNames.BearerToken);
	}

	private static async Task<Results<Ok<CharacterData>, ProblemHttpResult, InternalServerError>> HandleAsync(
		[FromBody] ModifyOwnCharacterRequestBody body,
		HttpContext httpContext,
		[FromServices] UserManager<User> userManager,
		[FromServices] IValidator<ModifyOwnCharacterRequestBody> bodyValidator,
		[FromServices] ApplicationDbContext dbContext,
		[FromServices] ILogger<ModifyOwnCharacter> logger)
	{
		var bodyValidation = await bodyValidator.ValidateAsync(body);
		if (!bodyValidation.IsValid)
		{
			var problemDetails = bodyValidation.Errors.ToProblemDetails();
			return TypedResults.Problem(problemDetails);
		}

		var userIdClaimValue = userManager.GetUserId(httpContext.User);
		if (!UInt64.TryParse(userIdClaimValue, out var userId))
		{
			return TypedResults.InternalServerError();
		}

		var character = await dbContext.Characters
			.AsTracking()
			.Where(character => character.Id == userId)
			.FirstOrDefaultAsync();
		if (character is null)
		{
			return TypedResults.InternalServerError();
		}

		character.Update(body.DisplayName, body.Description);

		try
		{
			_ = await dbContext.SaveChangesAsync();
		}
		catch (DbUpdateConcurrencyException)
		{
			return TypedResults.InternalServerError();
		}

		return TypedResults.Ok(new CharacterData
		{
			Id = character.Id,
			DisplayName = character.DisplayName,
			Description = character.Description,
			CurrentRoomId = character.CurrentRoomId,
			Presence = character.Presence,
			OwnerType = character.OwnerType
		});
	}
}
