using Aula.Server.Core.Domain.Users;
using Aula.Server.Core.Endpoints;
using Aula.Server.Core.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Aula.Server.Core.Features.Identity;

internal sealed class LogInEndpoint : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapPost("identity/log-in", HandleAsync)
			.HasApiVersion(1);
	}

	private static async Task<Results<Ok<LogInResponse>, ProblemHttpResult, EmptyHttpResult>> HandleAsync(
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
		if (user?.Type is not UserType.Standard)
		{
			return TypedResults.Problem(ProblemDetailsDefaults.UserDoesNotExist);
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

		return TypedResults.Ok(new LogInResponse
		{
			Token = tokenProvider.CreateToken(user),
		});
	}
}
