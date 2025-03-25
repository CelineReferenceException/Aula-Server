using Aula.Server.Common.Authorization;
using Aula.Server.Common.Endpoints;
using Aula.Server.Common.Persistence;
using Aula.Server.Domain;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Core.Api.Users;

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
		[FromServices] IValidator<SetPermissionsRequestBody> bodyValidator,
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

		user.Modify(permissions: body.Permissions).ThrowIfFailed();
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
