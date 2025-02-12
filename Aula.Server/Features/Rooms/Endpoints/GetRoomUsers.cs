using Aula.Server.Common.Endpoints;
using Aula.Server.Common.Persistence;
using Aula.Server.Common.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Features.Rooms.Endpoints;

internal sealed class GetRoomUsers : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapGet("/rooms/{roomId}/users", HandleAsync)
			.RequireRateLimiting(RateLimitPolicyNames.Global)
			.RequireAuthenticatedUser()
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<Ok<List<UserData>>, NotFound>> HandleAsync(
		[FromRoute] UInt64 roomId,
		[FromServices] ApplicationDbContext dbContext)
	{
		var roomExists = await dbContext.Rooms
			.AsNoTracking()
			.AnyAsync(r => r.Id == roomId);
		if (!roomExists)
		{
			return TypedResults.NotFound();
		}

		var users = await dbContext.Users
			.AsNoTracking()
			.Where(u => u.CurrentRoomId == roomId && !u.IsRemoved)
			.Select(u => new UserData
			{
				Id = u.Id,
				DisplayName = u.DisplayName,
				Description = u.Description,
				Type = u.Type,
				Presence = u.Presence,
				Permissions = u.Permissions,
				CurrentRoomId = u.CurrentRoomId,
			})
			.ToListAsync();

		return TypedResults.Ok(users);
	}
}
