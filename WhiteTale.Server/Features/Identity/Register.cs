using WhiteTale.Server.Domain.Characters;
using WhiteTale.Server.Domain.Users;

namespace WhiteTale.Server.Features.Identity;

internal sealed class Register : IEndpoint
{
	private const String ConfirmEmailRedirectUriQueryParameter = "confirmEmailRedirectUri";

	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapPost("api/identity/register", HandleAsync)
			.RequireRateLimiting(CommonRateLimitPolicyNames.Global);
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
		var bodyValidation = await bodyValidator.ValidateAsync(body);
		if (!bodyValidation.IsValid)
		{
			var problemDetails = bodyValidation.Errors.ToProblemDetails();
			return TypedResults.Problem(problemDetails);
		}

		var currentUser = await userManager.FindByEmailAsync(body.Email);
		if (currentUser is not null)
		{
			await resetPasswordEmailSender.SendEmailAsync(currentUser);
			return TypedResults.NoContent();
		}

		var newId = snowflakes.NewSnowflake();
		var newUser = new User(body.UserName)
		{
			Id = newId,
			Email = body.Email
		};

		var newCharacter = Character.Create(newId, body.DisplayName ?? body.UserName, CharacterOwnerType.Standard);

		var identityCreation = await userManager.CreateAsync(newUser, body.Password);
		if (!identityCreation.Succeeded)
		{
			var problemDetails = identityCreation.Errors.ToProblemDetails();
			return TypedResults.Problem(problemDetails);
		}

		_ = dbContext.Characters.Add(newCharacter);
		_ = await dbContext.SaveChangesAsync();

		await confirmEmailEmailSender.SendEmailAsync(newUser, httpRequest);
		return TypedResults.NoContent();
	}
}
