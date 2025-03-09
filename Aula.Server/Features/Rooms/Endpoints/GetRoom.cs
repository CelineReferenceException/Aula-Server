using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Features.Rooms.Endpoints;

internal sealed class GetRoom : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapGet("rooms/{roomId}", HandleAsync)
			.RequireAuthenticatedUser()
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<Ok<RoomData>, NotFound>> HandleAsync(
		[FromRoute] UInt64 roomId,
		[FromServices] ApplicationDbContext dbContext)
	{
		var room = await dbContext.Rooms
			.AsNoTracking()
			.Where(r => r.Id == roomId && !r.IsRemoved)
			.Select(r =>
				new RoomData
				{
					Id = r.Id,
					Name = r.Name,
					Description = r.Description,
					IsEntrance = r.IsEntrance,
					ConnectedRoomIds = Enumerable.Select(r.Connections, c => c.TargetRoomId).ToList(),
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
