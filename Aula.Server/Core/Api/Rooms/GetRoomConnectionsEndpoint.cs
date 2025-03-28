using Aula.Server.Common.Authorization;
using Aula.Server.Common.Persistence;
using Aula.Server.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Core.Api.Rooms;

internal sealed class GetRoomConnectionsEndpoint : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapGet("rooms/{roomId}/connections", HandleAsync)
			.RequireAuthenticatedUser()
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<Ok<RoomData[]>, ProblemHttpResult>> HandleAsync(
		[FromRoute] Snowflake roomId,
		[FromServices] ApplicationDbContext dbContext)
	{
		if (!await dbContext.Rooms.AnyAsync(r => r.Id == roomId && !r.IsRemoved))
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
				CreationTime = r.CreationDate,
			})
			.ToListAsync();

		var targetRoomConnections = await dbContext.RoomConnections
			.Where(c => targetRooms.Select(r => r.Id).Contains(c.SourceRoomId))
			.Select(c =>
				new
				{
					c.SourceRoomId,
					c.TargetRoomId,
				})
			.ToListAsync();

		var targetRoomsData = targetRooms
			.Select(r => new RoomData
			{
				Id = r.Id,
				Name = r.Name,
				Description = r.Description,
				IsEntrance = r.IsEntrance,
				ConnectedRoomIds = targetRoomConnections
					.Where(c => c.SourceRoomId == r.Id)
					.Select(c => c.TargetRoomId)
					.ToArray(),
				CreationDate = r.CreationTime,
			})
			.ToArray();
		return TypedResults.Ok(targetRoomsData);
	}
}
