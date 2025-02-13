using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Features.Rooms.Endpoints;

internal sealed class GetRoomConnections : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapGet("rooms/{roomId}/connections", HandleAsync)
			.RequireRateLimiting(RateLimitPolicyNames.Global)
			.RequireAuthenticatedUser()
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<Ok<List<RoomData>>, ProblemHttpResult>> HandleAsync(
		[FromRoute] UInt64 roomId,
		[FromServices] ApplicationDbContext dbContext)
	{
		var roomExists = await dbContext.Rooms
			.AsNoTracking()
			.AnyAsync(r => r.Id == roomId && !r.IsRemoved);
		if (!roomExists)
		{
			return TypedResults.Problem(ProblemDetailsDefaults.RoomDoesNotExist);
		}

		var connections = await dbContext.RoomConnections
			.Where(c => c.SourceRoomId == roomId)
			.Select(c => c.TargetRoomId)
			.ToListAsync();

		var targetRooms = await dbContext.Rooms
			.Where(r => connections.Contains(r.Id))
			.Select(r => new
			{
				r.Id,
				r.Name,
				r.Description,
				r.IsEntrance,
				r.CreationTime,
			})
			.ToListAsync();

		var targetRoomConnections = await dbContext.RoomConnections
			.AsNoTracking()
			.Where(c => targetRooms.Select(r => r.Id).Contains(c.SourceRoomId))
			.Select(c =>
				new
				{
					c.SourceRoomId,
					c.TargetRoomId,
				})
			.ToListAsync();

		List<RoomData> targetRoomsData = new(targetRooms.Count);
		foreach (var room in targetRooms)
		{
			targetRoomsData.Add(new RoomData
			{
				Id = room.Id,
				Name = room.Name,
				Description = room.Description,
				IsEntrance = room.IsEntrance,
				ConnectedRoomIds = targetRoomConnections
					.Where(c => c.SourceRoomId == room.Id)
					.Select(c => c.TargetRoomId)
					.ToList(),
				CreationTime = room.CreationTime,
			});
		}

		return TypedResults.Ok(targetRoomsData);
	}
}
