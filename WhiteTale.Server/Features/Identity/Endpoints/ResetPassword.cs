using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace WhiteTale.Server.Features.Identity.Endpoints;

internal sealed class ResetPassword : IEndpoint
{

	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapPost("identity/reset-password", HandleAsync)
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
		if (user?.Type is not UserType.Standard)
		{
			return TypedResults.Problem(ProblemDetailsDefaults.UserDoesNotExist);
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
