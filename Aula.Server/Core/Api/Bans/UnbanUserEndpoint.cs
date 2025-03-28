using Aula.Server.Common.Authorization;
using Aula.Server.Common.Persistence;
using Aula.Server.Domain;
using Aula.Server.Domain.Users;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Core.Api.Bans;

internal sealed class UnbanUserEndpoint : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapDelete("bans/users/{userId}", HandleAsync)
			.RequireAuthenticatedUser()
			.RequirePermissions(Permissions.BanUsers)
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<NoContent, ProblemHttpResult, InternalServerError>> HandleAsync(
		[FromRoute] Snowflake userId,
		[FromServices] ApplicationDbContext dbContext)
	{
		var ban = await dbContext.Bans
			.AsTracking()
			.Where(x => x.TargetId == userId)
			.FirstOrDefaultAsync();
		if (ban is null)
		{
			return TypedResults.NoContent();
		}

		ban.Remove();
		_ = dbContext.Bans.Remove(ban);

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
