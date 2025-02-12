using Aula.Server.Common.Endpoints;
using Aula.Server.Common.Persistence;
using Aula.Server.Common.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Features.Users.Endpoints;

internal sealed class GetUser : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapGet("users/{userId}", HandleAsync)
			.RequireRateLimiting(RateLimitPolicyNames.Global)
			.RequireAuthenticatedUser()
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<Ok<UserData>, NotFound>> HandleAsync(
		[FromRoute] UInt64 userId,
		[FromServices] ApplicationDbContext dbContext)
	{
		var user = await dbContext.Users
			.AsNoTracking()
			.Where(u => u.Id == userId && !u.IsRemoved)
			.Select(u =>
				new UserData
				{
					Id = userId,
					DisplayName = u.DisplayName,
					Description = u.Description,
					CurrentRoomId = u.CurrentRoomId,
					Type = u.Type,
					Presence = u.Presence,
					Permissions = u.Permissions,
				})
			.FirstOrDefaultAsync();
		if (user is null)
		{
			return TypedResults.NotFound();
		}

		return TypedResults.Ok(user);
	}
}
