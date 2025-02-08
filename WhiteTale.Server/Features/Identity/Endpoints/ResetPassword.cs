using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace WhiteTale.Server.Features.Identity.Endpoints;

internal sealed class ResetPassword : IEndpoint
{
	internal const String Route = "api/identity/resetpassword";

	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapPost(Route, HandleAsync)
			.RequireRateLimiting(RateLimitPolicyNames.Global)
			.HasApiVersion(1);
	}

	private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
		[FromBody] ResetPasswordRequestBody body,
		[FromServices] ResetPasswordRequestBodyValidator bodyValidator,
		[FromServices] UserManager userManager)
	{
		var bodyValidation = await bodyValidator.ValidateAsync(body);
		if (!bodyValidation.IsValid)
		{
			var problemDetails = bodyValidation.Errors.ToProblemDetails();
			return TypedResults.Problem(problemDetails);
		}

		var user = await userManager.FindByIdAsync(body.UserId);
		if (user is null)
		{
			return TypedResults.Problem(new ProblemDetails
			{
				Title = "User not found",
				Detail = "There is no user with the given ID.",
				Status = StatusCodes.Status404NotFound,
			});
		}

		var passwordReset = await userManager.ResetPasswordAsync(user, body.NewPassword, body.ResetToken);
		if (!passwordReset.Succeeded)
		{
			return TypedResults.Problem(new ProblemDetails
			{
				Title = "Password problem",
				Detail = passwordReset.ToString(),
				Status = StatusCodes.Status400BadRequest,
			});
		}

		return TypedResults.NoContent();
	}
}
