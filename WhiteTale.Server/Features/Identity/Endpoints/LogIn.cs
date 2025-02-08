using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace WhiteTale.Server.Features.Identity.Endpoints;

internal sealed class LogIn : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapPost("identity/login", HandleAsync)
			.RequireRateLimiting(RateLimitPolicyNames.Global)
			.HasApiVersion(1);
	}

	private static async Task<Results<Ok<AccessTokenData>, ProblemHttpResult, EmptyHttpResult>> HandleAsync(
		[FromBody] LogInRequestBody body,
		[FromServices] LogInRequestBodyValidator bodyValidator,
		[FromServices] UserManager userManager,
		[FromServices] TokenProvider tokenProvider)
	{
		var bodyValidation = await bodyValidator.ValidateAsync(body);
		if (!bodyValidation.IsValid)
		{
			var problemDetails = bodyValidation.Errors.ToProblemDetails();
			return TypedResults.Problem(problemDetails);
		}

		var user = await userManager.FindByUserNameAsync(body.UserName);
		if (user is null)
		{
			return TypedResults.Problem(new ProblemDetails
			{
				Title = "Unknown user",
				Status = StatusCodes.Status400BadRequest,
			});
		}

		var isPasswordCorrect = userManager.CheckPassword(user, body.Password);
		if (!isPasswordCorrect)
		{
			await userManager.AccessFailedAsync(user);
			return TypedResults.Problem(new ProblemDetails
			{
				Title = "A login problem has occurred",
				Detail = "The password provided is incorrect.",
				Status = StatusCodes.Status400BadRequest,
			});
		}

		if (user.LockoutEndTime > DateTime.UtcNow)
		{
			return TypedResults.Problem(new ProblemDetails
			{
				Title = "A login problem has occurred",
				Detail = "The account is temporarily locked out due to multiple unsuccessful login attempts.",
				Status = StatusCodes.Status403Forbidden,
			});
		}

		if (userManager.Options.SignIn.RequireConfirmedEmail &&
		    !user.EmailConfirmed)
		{
			return TypedResults.Problem(new ProblemDetails
			{
				Title = "Email is not confirmed",
				Detail = "Email confirmation is required to login.",
				Status = StatusCodes.Status403Forbidden,
			});
		}

		return TypedResults.Ok(new AccessTokenData
		{
			Token = tokenProvider.CreateToken(user),
		});
	}
}
