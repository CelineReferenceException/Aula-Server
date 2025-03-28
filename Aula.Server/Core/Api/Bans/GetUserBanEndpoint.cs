using Aula.Server.Common.Authorization;
using Aula.Server.Common.Persistence;
using Aula.Server.Domain;
using Aula.Server.Domain.Bans;
using Aula.Server.Domain.Users;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Core.Api.Bans;

internal sealed class GetUserBanEndpoint : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapGet("bans/users/{userId}", HandleAsync)
			.RequireAuthenticatedUser()
			.RequirePermissions(Permissions.BanUsers)
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<Ok<BanData>, NotFound>> HandleAsync(
		[FromRoute] Snowflake userId,
		[FromServices] ApplicationDbContext dbContext)
	{
		var ban = await dbContext.Bans
			.Where(b => b.TargetId == userId)
			.Select(b => new BanData
			{
				Type = BanType.Id,
				ExecutorId = b.ExecutorId,
				Reason = b.Reason,
				TargetId = b.TargetId,
				CreationDate = b.CreationDate,
			})
			.FirstOrDefaultAsync();
		if (ban is null)
		{
			return TypedResults.NotFound();
		}

		return TypedResults.Ok(ban);
	}
}
