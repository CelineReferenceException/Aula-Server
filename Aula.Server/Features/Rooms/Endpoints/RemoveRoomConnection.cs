using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Features.Rooms.Endpoints;

internal sealed class RemoveRoomConnection : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapDelete("rooms/{sourceRoomId}/connections/{targetRoomId}", HandleAsync)
			.RequireAuthenticatedUser()
			.RequirePermissions(Permissions.ManageRooms)
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
		[FromRoute] UInt64 sourceRoomId,
		[FromRoute] UInt64 targetRoomId,
		[FromServices] ApplicationDbContext dbContext,
		[FromServices] SnowflakeGenerator snowflakeGenerator)
	{
		if (!await dbContext.Rooms.AnyAsync(r => r.Id == sourceRoomId && !r.IsRemoved))
		{
			return TypedResults.Problem(ProblemDetailsDefaults.RoomDoesNotExist);
		}

		if (!dbContext.Rooms.Any(r => r.Id == targetRoomId && !r.IsRemoved))
		{
			return TypedResults.Problem(ProblemDetailsDefaults.TargetRoomDoesNotExist);
		}

		var connection = await dbContext.RoomConnections
			.AsTracking()
			.Where(c => c.SourceRoomId == sourceRoomId && c.TargetRoomId == targetRoomId)
			.FirstOrDefaultAsync();
		if (connection is null)
		{
			return TypedResults.NoContent();
		}

		connection.Remove();
		_ = dbContext.RoomConnections.Remove(connection);
		_ = await dbContext.SaveChangesAsync();

		return TypedResults.NoContent();
	}
}
