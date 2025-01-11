using WhiteTale.Server.Domain.Characters;
using WhiteTale.Server.Domain.Users;

namespace WhiteTale.Server.Features.Identity;

internal sealed class Register : IEndpoint
{
	private const String ConfirmEmailRedirectUriQueryParameter = "confirmEmailRedirectUri";

	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapPost("api/identity/register", HandleAsync)
			.RequireRateLimiting(RateLimitPolicyNames.Global);
	}

	private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
		[FromBody] RegisterRequestBody body,
		[FromServices] IValidator<RegisterRequestBody> bodyValidator,
		[FromServices] ISnowflakeGenerator snowflakes,
		[FromServices] UserManager<User> userManager,
		[FromServices] ApplicationDbContext dbContext,
		HttpRequest httpRequest,
		[FromServices] ConfirmEmailEmailSender confirmEmailEmailSender,
		[FromServices] ResetPasswordEmailSender resetPasswordEmailSender)
	{
		var bodyValidation = await bodyValidator.ValidateAsync(body).ConfigureAwait(false);
		if (!bodyValidation.IsValid)
		{
			var problemDetails = bodyValidation.Errors.ToProblemDetails();
			return TypedResults.Problem(problemDetails);
		}

		var currentUser = await userManager.FindByEmailAsync(body.Email).ConfigureAwait(false);
		if (currentUser is not null)
		{
			await resetPasswordEmailSender.SendEmailAsync(currentUser).ConfigureAwait(false);
			return TypedResults.NoContent();
		}

		var newId = snowflakes.NewSnowflake();
		var newUser = new User(body.UserName)
		{
			Id = newId,
			Email = body.Email
		};

		var newCharacter = new Character
		{
			Id = newId,
			DisplayName = body.DisplayName ?? body.UserName,
			OwnerType = CharacterOwnerType.Standard,
			CreationTime = DateTime.UtcNow,
			ConcurrencyStamp = Guid.NewGuid().ToString()
		};

		var identityCreation = await userManager.CreateAsync(newUser, body.Password).ConfigureAwait(false);
		if (!identityCreation.Succeeded)
		{
			var problemDetails = identityCreation.Errors.ToProblemDetails();
			return TypedResults.Problem(problemDetails);
		}

		_ = dbContext.Characters.Add(newCharacter);
		_ = await dbContext.SaveChangesAsync().ConfigureAwait(false);

		await confirmEmailEmailSender.SendEmailAsync(newUser, httpRequest).ConfigureAwait(false);
		return TypedResults.NoContent();
	}
}
