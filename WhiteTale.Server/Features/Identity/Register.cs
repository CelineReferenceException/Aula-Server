using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace WhiteTale.Server.Features.Identity;

internal sealed class Register : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapPost("identity/register", HandleAsync)
			.RequireRateLimiting(CommonRateLimitPolicyNames.Global)
			.HasApiVersion(1);
	}

	private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
		[FromBody] RegisterRequestBody body,
		[FromServices] IValidator<RegisterRequestBody> bodyValidator,
		[FromServices] SnowflakeGenerator snowflakes,
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

		// TODO: Get default permissions from configuration.
		var newUser = User.Create(snowflakes.NewSnowflake(), body.Email, body.UserName, body.DisplayName, UserOwnerType.Standard, 0);

		var identityCreation = await userManager.CreateAsync(newUser, body.Password);
		if (!identityCreation.Succeeded)
		{
			var problemDetails = identityCreation.Errors.ToProblemDetails();
			return TypedResults.Problem(problemDetails);
		}

		_ = await dbContext.SaveChangesAsync();

		await confirmEmailEmailSender.SendEmailAsync(newUser, httpRequest);
		return TypedResults.NoContent();
	}
}
