using Aula.Server.Core.Authorization;
using Aula.Server.Core.Domain;
using Aula.Server.Core.Endpoints;
using Aula.Server.Core.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Core.Features.Users;

internal sealed class SetPermissionsEndpoint : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapPut("users/{userId}/permissions", HandleAsync)
			.RequireAuthenticatedUser()
			.DenyBannedUsers()
			.RequirePermissions();
	}

	private static async Task<Results<NoContent, ProblemHttpResult, InternalServerError>> HandleAsync(
		[FromRoute] Snowflake userId,
		[FromBody] SetPermissionsRequestBody body,
		[FromServices] SetPermissionsRequestBodyValidator bodyValidator,
		[FromServices] ApplicationDbContext dbContext)
	{
		var validation = await bodyValidator.ValidateAsync(body);
		if (!validation.IsValid)
		{
			var problemDetails = validation.Errors.ToProblemDetails();
			return TypedResults.Problem(problemDetails);
		}

		var user = await dbContext.Users
			.AsTracking()
			.Where(u => u.Id == userId && !u.IsRemoved)
			.FirstOrDefaultAsync();
		if (user is null)
		{
			return TypedResults.Problem(ProblemDetailsDefaults.UserDoesNotExist);
		}

		user.Modify(permissions: body.Permissions);
		user.UpdateConcurrencyStamp();

		try
		{
			_ = await dbContext.SaveChangesAsync();
		}
		catch (DbUpdateConcurrencyException)
		{
			return TypedResults.InternalServerError();
		}

		return TypedResults.NoContent();
	}
}
