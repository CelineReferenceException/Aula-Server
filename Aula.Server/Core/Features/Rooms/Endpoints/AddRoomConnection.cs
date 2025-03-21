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

internal sealed class AddRoomConnection : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapPut("rooms/{roomId}/connections/{targetId}", HandleAsync)
			.RequireAuthenticatedUser()
			.RequirePermissions(Permissions.ManageRooms)
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
		[FromRoute] UInt64 roomId,
		[FromRoute] UInt64 targetId,
		[FromServices] ApplicationDbContext dbContext,
		[FromServices] SnowflakeGenerator snowflakeGenerator)
	{
		if (roomId == targetId)
		{
			return TypedResults.Problem(ProblemDetailsDefaults.TargetRoomCannotBeSourceRoom);
		}

		if (!await dbContext.Rooms.AnyAsync(r => r.Id == roomId && !r.IsRemoved))
		{
			return TypedResults.Problem(ProblemDetailsDefaults.RoomDoesNotExist);
		}

		if (!await dbContext.Rooms.AnyAsync(r => r.Id == targetId && !r.IsRemoved))
		{
			return TypedResults.Problem(ProblemDetailsDefaults.TargetRoomDoesNotExist);
		}

		if (await dbContext.RoomConnections.AnyAsync(r => r.SourceRoomId == roomId && r.TargetRoomId == targetId))
		{
			return TypedResults.NoContent();
		}

		var roomConnection = RoomConnection.Create(await snowflakeGenerator.NewSnowflakeAsync(), roomId, targetId);

		_ = await dbContext.AddAsync(roomConnection);
		_ = await dbContext.SaveChangesAsync();

		return TypedResults.NoContent();
	}
}
