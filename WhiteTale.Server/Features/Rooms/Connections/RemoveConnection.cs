using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace WhiteTale.Server.Features.Rooms.Connections;

internal sealed class RemoveConnection : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapDelete("api/rooms/{sourceRoomId}/connections/{targetRoomId}", HandleAsync)
			.RequireRateLimiting(CommonRateLimitPolicyNames.Global)
			.RequireAuthorization(IdentityAuthorizationPolicyNames.BearerToken)
			.RequirePermission(Permissions.ManageRooms);
	}

	private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
		[FromRoute] UInt64 sourceRoomId,
		[FromRoute] UInt64 targetRoomId,
		[FromServices] ApplicationDbContext dbContext,
		[FromServices] SnowflakeGenerator snowflakeGenerator)
	{
		var sourceRoomExists = await dbContext.Rooms
			.AsNoTracking()
			.AnyAsync(room => room.Id == sourceRoomId && !room.IsRemoved);
		if (!sourceRoomExists)
		{
			return TypedResults.Problem(new ProblemDetails
			{
				Title = "Invalid room",
				Detail = "The room does not exist.",
			});
		}

		var targetRoomExists = dbContext.Rooms
			.AsNoTracking()
			.Any(room => room.Id == targetRoomId && !room.IsRemoved);
		if (!targetRoomExists)
		{
			return TypedResults.Problem(new ProblemDetails
			{
				Title = "Invalid target room",
				Detail = "The target room does not exist.",
			});
		}

		var connection = await dbContext.RoomConnections
			.Where(connection => connection.SourceRoomId == sourceRoomId && connection.TargetRoomId == targetRoomId)
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
