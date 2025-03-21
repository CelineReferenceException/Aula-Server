using Aula.Server.Core.Authorization;
using Aula.Server.Core.Endpoints;
using Aula.Server.Core.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Core.Features.Rooms;

internal sealed class GetRoomUsersEndpoint : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapGet("/rooms/{roomId}/users", HandleAsync)
			.RequireAuthenticatedUser()
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<Ok<List<UserData>>, NotFound>> HandleAsync(
		[FromRoute] Snowflake roomId,
		[FromServices] ApplicationDbContext dbContext)
	{
		if (!await dbContext.Rooms.AnyAsync(r => r.Id == roomId))
		{
			return TypedResults.NotFound();
		}

		var users = await dbContext.Users
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
