using Aula.Server.Core.Authorization;
using Aula.Server.Core.Endpoints;
using Aula.Server.Core.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Core.Features.Rooms.Endpoints;

internal sealed class SetRoomConnections : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapPut("rooms/{roomId}/connections", HandleAsync)
			.RequireAuthenticatedUser()
			.RequirePermissions(Permissions.ManageRooms)
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<NoContent, ProblemHttpResult, InternalServerError>> HandleAsync(
		[FromRoute] UInt64 roomId,
		[FromBody] SetRoomConnectionsRequestBody body,
		[FromServices] ApplicationDbContext dbContext,
		[FromServices] SnowflakeGenerator snowflakeGenerator)
	{
		if (body.RoomIds.Contains(roomId))
		{
			return TypedResults.Problem(ProblemDetailsDefaults.TargetRoomCannotBeSourceRoom);
		}

		if (!await dbContext.Rooms.AnyAsync(r => r.Id == roomId && !r.IsRemoved))
		{
			return TypedResults.Problem(ProblemDetailsDefaults.RoomDoesNotExist);
		}

		if (!dbContext.Rooms.Any(r => body.RoomIds.Contains(r.Id) && !r.IsRemoved))
		{
			return TypedResults.Problem(ProblemDetailsDefaults.TargetRoomDoesNotExist);
		}

		var alreadyConnectedTargetIds = await dbContext.RoomConnections
			.Where(c => c.SourceRoomId == roomId && body.RoomIds.Contains(c.TargetRoomId))
			.Select(c => c.TargetRoomId)
			.ToListAsync();

		var newConnections = body.RoomIds
			.Where(targetId => !alreadyConnectedTargetIds.Contains(targetId))
			.Select(targetId => new RoomConnection(snowflakeGenerator.NewSnowflakeAsync().AsTask().Result, roomId, targetId))
			.ToArray();

		var targetsToRemove = await dbContext.RoomConnections
			.AsTracking()
			.Where(c => c.SourceRoomId == roomId && !body.RoomIds.Contains(c.TargetRoomId))
			.ToListAsync();

		dbContext.RoomConnections.AddRange(newConnections);
		dbContext.RoomConnections.RemoveRange(targetsToRemove);

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
