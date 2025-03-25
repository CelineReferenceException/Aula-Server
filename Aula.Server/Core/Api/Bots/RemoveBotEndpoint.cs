using Aula.Server.Common.Authorization;
using Aula.Server.Common.Endpoints;
using Aula.Server.Common.Persistence;
using Aula.Server.Domain;
using Aula.Server.Domain.Users;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Core.Api.Bots;

internal sealed class RemoveBotEndpoint : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapDelete("bots/{userId}", HandleAsync)
			.RequireAuthenticatedUser()
			.RequireUserType(UserType.Standard)
			.RequirePermissions()
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<NoContent, ProblemHttpResult, InternalServerError>> HandleAsync(
		[FromRoute] Snowflake userId,
		[FromServices] ApplicationDbContext dbContext)
	{
		var user = await dbContext.Users
			.AsTracking()
			.Where(u => u.Id == userId && u.Type == UserType.Bot && !u.IsRemoved)
			.FirstOrDefaultAsync();
		if (user is null)
		{
			return TypedResults.Problem(ProblemDetailsDefaults.UserDoesNotExist);
		}

		user.Remove();

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
