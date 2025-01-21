using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace WhiteTale.Server.Features.Identity;

internal sealed class ResetPassword : IEndpoint
{
	internal const String Route = "api/identity/resetpassword";

	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapPost(Route, HandleAsync)
			.RequireRateLimiting(CommonRateLimitPolicyNames.Global);
	}

	private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
		[FromBody] ResetPasswordRequestBody body,
		[FromServices] IValidator<ResetPasswordRequestBody> bodyValidator,
		[FromServices] UserManager<User> userManager)
	{
		var bodyValidation = await bodyValidator.ValidateAsync(body);
		if (!bodyValidation.IsValid)
		{
			var problemDetails = bodyValidation.Errors.ToProblemDetails();
			return TypedResults.Problem(problemDetails);
		}

		var user = await userManager.FindByIdAsync(body.UserId.ToString());
		if (user is null)
		{
			return TypedResults.Problem(new ProblemDetails
			{
				Title = "User not found",
				Detail = "There is no user with the given ID.",
				Status = StatusCodes.Status404NotFound,
			});
		}

		var passwordReset = await userManager.ResetPasswordAsync(user, body.ResetToken, body.NewPassword);
		if (!passwordReset.Succeeded)
		{
			var problemDetails = passwordReset.Errors.ToProblemDetails();
			return TypedResults.Problem(problemDetails);
		}

		return TypedResults.NoContent();
	}
}
