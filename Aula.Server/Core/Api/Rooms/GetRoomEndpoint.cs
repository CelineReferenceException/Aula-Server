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

namespace Aula.Server.Core.Api.Rooms;

internal sealed class GetRoomEndpoint : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapGet("rooms/{roomId}", HandleAsync)
			.RequireAuthenticatedUser()
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<Ok<RoomData>, NotFound>> HandleAsync(
		[FromRoute] Snowflake roomId,
		[FromServices] ApplicationDbContext dbContext)
	{
		var room = await dbContext.Rooms
			.Where(r => r.Id == roomId && !r.IsRemoved)
			.Select(r =>
				new RoomData
				{
					Id = r.Id,
					Name = r.Name,
					Description = r.Description,
					IsEntrance = r.IsEntrance,
					ConnectedRoomIds = Enumerable.Select(r.Connections, c => c.TargetRoomId).ToArray(),
					CreationDate = r.CreationDate,
				})
			.FirstOrDefaultAsync();
		if (room is null)
		{
			return TypedResults.NotFound();
		}

		return TypedResults.Ok(room);
	}
}
