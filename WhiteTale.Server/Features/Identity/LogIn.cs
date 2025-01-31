using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace WhiteTale.Server.Features.Identity;

internal sealed class LogIn : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapPost("identity/login", HandleAsync)
			.RequireRateLimiting(RateLimitPolicyNames.Global)
			.HasApiVersion(1);
	}

	private static async Task<Results<Ok<AccessTokenResponse>, ProblemHttpResult, EmptyHttpResult>> HandleAsync(
		[FromBody] LogInRequestBody body,
		[FromServices] IValidator<LogInRequestBody> bodyValidator,
		[FromServices] UserManager<User> userManager,
		[FromServices] SignInManager<User> signInManager)
	{
		var bodyValidation = await bodyValidator.ValidateAsync(body);
		if (!bodyValidation.IsValid)
		{
			var problemDetails = bodyValidation.Errors.ToProblemDetails();
			return TypedResults.Problem(problemDetails);
		}

		var user = await userManager.FindByNameAsync(body.UserName);
		if (user is null)
		{
			return TypedResults.Problem(new ProblemDetails
			{
				Title = "Unknown user",
				Status = StatusCodes.Status400BadRequest,
			});
		}

		var isPasswordCorrect = await userManager.CheckPasswordAsync(user, body.Password);

		if (!isPasswordCorrect)
		{
			_ = await userManager.AccessFailedAsync(user);
			return TypedResults.Problem(new ProblemDetails
			{
				Title = "A login problem has occurred",
				Detail = "The password provided is incorrect.",
				Status = StatusCodes.Status400BadRequest,
			});
		}

		if (await userManager.IsLockedOutAsync(user))
		{
			return TypedResults.Problem(new ProblemDetails
			{
				Title = "A login problem has occurred",
				Detail = "The account is temporarily locked out due to multiple unsuccessful login attempts.",
				Status = StatusCodes.Status403Forbidden,
			});
		}

		if (userManager.Options.SignIn.RequireConfirmedEmail &&
		    !await userManager.IsEmailConfirmedAsync(user))
		{
			return TypedResults.Problem(new ProblemDetails
			{
				Title = "Email is not confirmed",
				Detail = "Email confirmation is required to login.",
				Status = StatusCodes.Status403Forbidden,
			});
		}

		signInManager.AuthenticationScheme = IdentityConstants.BearerScheme;
		await signInManager.SignInAsync(user, new AuthenticationProperties());

		// The signInManager already produced a response in the form of a bearer token.
		return TypedResults.Empty;
	}
}
