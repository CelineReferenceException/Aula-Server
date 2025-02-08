using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace WhiteTale.Server.Features.RoomConnections.Endpoints;

internal sealed class RemoveRoomConnection : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapDelete("rooms/{sourceRoomId}/connections/{targetRoomId}", HandleAsync)
			.RequireRateLimiting(RateLimitPolicyNames.Global)
			.RequireAuthenticatedUser()
			.RequirePermissions(Permissions.ManageRooms)
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
		[FromRoute] UInt64 sourceRoomId,
		[FromRoute] UInt64 targetRoomId,
		[FromServices] ApplicationDbContext dbContext,
		[FromServices] SnowflakeProvider snowflakeProvider)
	{
		var sourceRoomExists = await dbContext.Rooms
			.AsNoTracking()
			.AnyAsync(r => r.Id == sourceRoomId && !r.IsRemoved);
		if (!sourceRoomExists)
		{
			return TypedResults.Problem(ProblemDetailsDefaults.RoomDoesNotExist);
		}

		var targetRoomExists = dbContext.Rooms
			.AsNoTracking()
			.Any(r => r.Id == targetRoomId && !r.IsRemoved);
		if (!targetRoomExists)
		{
			return TypedResults.Problem(ProblemDetailsDefaults.TargetRoomDoesNotExist);
		}

		var connection = await dbContext.RoomConnections
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
