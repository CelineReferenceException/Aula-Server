using Aula.Server.Core.Endpoints;
using Aula.Server.Core.Identity;
using Aula.Server.Core.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Core.Features.Identity;

internal sealed class ResetTokenEndpoint : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapPost("identity/reset-token", HandleAsync)
			.HasApiVersion(1);
	}

	private static async Task<Results<NoContent, ProblemHttpResult, EmptyHttpResult>> HandleAsync(
		[FromBody] LogInRequestBody body,
		[FromServices] LogInRequestBodyValidator bodyValidator,
		[FromServices] UserManager userManager,
		[FromServices] ApplicationDbContext dbContext)
	{
		var bodyValidation = await bodyValidator.ValidateAsync(body);
		if (!bodyValidation.IsValid)
		{
			var problemDetails = bodyValidation.Errors.ToProblemDetails();
			return TypedResults.Problem(problemDetails);
		}

		// We fetch the user entity so we don't modify the cached one by the UserManager
		var user = await dbContext.Users
			.AsTracking()
			.Where(u => u.UserName == body.UserName && !u.IsRemoved)
			.FirstOrDefaultAsync();
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

		user.UpdateSecurityStamp();
		_ = dbContext.SaveChangesWithConcurrencyCheckBypassAsync();

		return TypedResults.NoContent();
	}
}
