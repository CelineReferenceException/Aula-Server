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
		_ = route.MapPost("identity/log-in", HandleAsync)
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
		if (user?.OwnerType is not UserOwnerType.Standard)
		{
			return TypedResults.Problem(ProblemDetailsDefaults.UnknownUser);
		}

		var isPasswordCorrect = userManager.CheckPassword(user, body.Password);
		if (!isPasswordCorrect)
		{
			await userManager.AccessFailedAsync(user);
			return TypedResults.Problem(ProblemDetailsDefaults.IncorrectPassword);
		}

		if (user.LockoutEndTime > DateTime.UtcNow)
		{
			return TypedResults.Problem(ProblemDetailsDefaults.UserIsLockedOut);
		}

		if (userManager.Options.SignIn.RequireConfirmedEmail &&
		    !user.EmailConfirmed)
		{
			return TypedResults.Problem(ProblemDetailsDefaults.EmailNotConfirmed);
		}

		return TypedResults.Ok(new AccessTokenData
		{
			Token = tokenProvider.CreateToken(user),
		});
	}
}
