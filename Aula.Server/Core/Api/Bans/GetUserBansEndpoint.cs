using Aula.Server.Common.Authorization;
using Aula.Server.Common.Endpoints;
using Aula.Server.Common.Persistence;
using Aula.Server.Domain.Bans;
using Aula.Server.Domain.Users;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Core.Api.Bans;

internal sealed class GetUserBansEndpoint : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapGet("bans/users", HandleAsync)
			.RequireAuthenticatedUser()
			.RequirePermissions(Permissions.BanUsers)
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Ok<List<BanData>>> HandleAsync([FromServices] ApplicationDbContext dbContext)
	{
		var bans = await dbContext.Bans
			.Where(b => b.Type == BanType.Id)
			.Select(b => new BanData
			{
				Type = b.Type,
				ExecutorId = b.ExecutorId,
				Reason = b.Reason,
				TargetId = b.TargetId,
				CreationDate = b.CreationDate,
			})
			.ToListAsync();

		return TypedResults.Ok(bans);
	}
}
