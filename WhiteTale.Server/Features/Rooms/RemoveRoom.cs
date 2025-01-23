using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace WhiteTale.Server.Features.Rooms;

internal sealed class RemoveRoom : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapDelete("api/rooms/{roomId}", HandleAsync)
			.RequireRateLimiting(CommonRateLimitPolicyNames.Global)
			.RequireAuthorization(IdentityAuthorizationPolicyNames.BearerToken)
			.RequirePermission(Permissions.ManageRooms);
	}

	private static async Task<Results<Ok<RoomData>, NotFound, InternalServerError>> HandleAsync(
		[FromRoute] UInt64 roomId,
		[FromServices] ApplicationDbContext dbContext)
	{
		var room = await dbContext.Rooms
			.AsTracking()
			.Where(r => r.Id == roomId && !r.IsRemoved)
			.FirstOrDefaultAsync();
		if (room is null)
		{
			return TypedResults.NotFound();
		}

		room.Remove();

		var connections = await dbContext.RoomConnections
			.AsTracking()
			.Where(connection => connection.SourceRoomId == roomId || connection.TargetRoomId == roomId)
			.ToListAsync();

		connections.ForEach(connection => connection.Remove());
		dbContext.RoomConnections.RemoveRange(connections);

		var usersInRoom = await dbContext.Users
			.AsTracking()
			.Where(user => user.CurrentRoomId == roomId)
			.ToListAsync();

		usersInRoom.ForEach(user => user.SetCurrentRoom(null));

		try
		{
			_ = await dbContext.SaveChangesAsync();
		}
		catch (DbUpdateConcurrencyException)
		{
			return TypedResults.InternalServerError();
		}

		return TypedResults.Ok(new RoomData
		{
			Id = room.Id,
			Name = room.Name,
			Description = room.Description,
			IsEntrance = room.IsEntrance,
			CreationTime = room.CreationTime,
		});
	}
}
