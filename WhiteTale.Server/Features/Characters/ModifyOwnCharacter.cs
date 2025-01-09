using WhiteTale.Server.Common.Identity;
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
		[FromServices] HttpContext httpContext,
		[FromServices] UserManager<User> userManager,
		[FromServices] IValidator<ModifyOwnCharacterRequestBody> bodyValidator,
		[FromServices] ApplicationDbContext dbContext,
		[FromServices] ILogger logger)
	{
		var bodyValidation = await bodyValidator.ValidateAsync(body).ConfigureAwait(false);
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
			.FirstOrDefaultAsync()
			.ConfigureAwait(false);
		if (character is null)
		{
			return TypedResults.InternalServerError();
		}

		character.DisplayName = body.DisplayName ?? character.DisplayName;
		character.Description = body.Description ?? character.Description;
		character.ConcurrencyStamp = Guid.NewGuid().ToString();

		try
		{
			_ = await dbContext.SaveChangesAsync().ConfigureAwait(false);
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
			OwnerType = character.OwnerType,
			Permissions = character.Permissions
		});
	}
}
