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
		[FromServices] UserManager userManager,
		[FromServices] TokenProvider tokenProvider)
	{
		var bodyValidation = await bodyValidator.ValidateAsync(body);
		if (!bodyValidation.IsValid)
		{
			var problemDetails = bodyValidation.Errors.ToProblemDetails();
			return TypedResults.Problem(problemDetails);
		}

		if (!tokenProvider.TryReadFromToken(body.Code, out var userIdString, out var resetToken) ||
		    !UInt64.TryParse(userIdString, out var userId))
		{
			return TypedResults.Problem(ProblemDetailsDefaults.InvalidResetPasswordToken);
		}

		var user = await userManager.FindByIdAsync(userId);
		if (user?.Type is not UserType.Standard)
		{
			return TypedResults.Problem(ProblemDetailsDefaults.InvalidResetPasswordToken);
		}

		var passwordReset = await userManager.ResetPasswordAsync(user, body.NewPassword, resetToken);
		if (passwordReset == ResetPasswordResult.InvalidToken)
		{
			return TypedResults.Problem(ProblemDetailsDefaults.InvalidResetPasswordToken);
		}
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
