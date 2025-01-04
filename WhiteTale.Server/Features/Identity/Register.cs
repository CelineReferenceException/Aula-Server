using System.Text;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using WhiteTale.Server.Common.RateLimiting;
using WhiteTale.Server.Domain.Characters;
using WhiteTale.Server.Domain.Users;

namespace WhiteTale.Server.Features.Identity;

internal sealed class Register : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapPost("api/identity/register", HandleAsync)
			.RequireRateLimiting(RateLimitPolicyNames.Global);
	}

	private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
		[FromBody] RegisterRequestBody body,
		[FromServices] HttpRequest httpRequest,
		[FromServices] IValidator<RegisterRequestBody> bodyValidator,
		[FromServices] ISnowflakeGenerator snowflakes,
		[FromServices] UserManager<User> userManager,
		[FromServices] ApplicationDbContext dbContext,
		[FromServices] IEmailSender emailSender,
		[FromServices] IOptions<ApplicationOptions> applicationOptions)
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
			var resetToken = await userManager.GeneratePasswordResetTokenAsync(currentUser).ConfigureAwait(false);
			resetToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(resetToken));
			var applicationName = applicationOptions.Value.Name;
			var content =
				$"""
				 <p>It looks like someone tried to create a new account using your email address on {applicationName}.
				 However, an account with this email already exists.</p>
				 <p>If you forgot your password, Here's your user ID and a reset token:</p>
				 <ul>
				 	<li><strong>User ID:</strong> <code>{currentUser.Id}</code></li>
				 	<li><strong>Reset Token:</strong> <code>{resetToken}</code></li>
				 </ul>
				 """;

			await emailSender.SendEmailAsync(body.Email, "Email already registered", content).ConfigureAwait(false);
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

		return TypedResults.NoContent();
	}
}
