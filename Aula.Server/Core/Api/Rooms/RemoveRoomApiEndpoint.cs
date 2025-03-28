using Aula.Server.Common.Authorization;
using Aula.Server.Common.Persistence;
using Aula.Server.Domain;
using Aula.Server.Domain.Users;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Core.Api.Rooms;

internal sealed class RemoveRoomApiEndpoint : IApiEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapDelete("rooms/{roomId}", HandleAsync)
			.RequireAuthenticatedUser()
			.RequirePermissions(Permissions.ManageRooms)
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<NoContent, InternalServerError>> HandleAsync(
		[FromRoute] Snowflake roomId,
		[FromServices] ApplicationDbContext dbContext)
	{
		var room = await dbContext.Rooms
			.AsTracking()
			.Where(r => r.Id == roomId && !r.IsRemoved)
			.FirstOrDefaultAsync();
		if (room is null)
		{
			return TypedResults.NoContent();
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

		return TypedResults.NoContent();
	}
}
