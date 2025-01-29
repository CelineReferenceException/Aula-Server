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
		_ = route.MapDelete("rooms/{sourceRoomId}/connections/{targetRoomId}", HandleAsync)
			.RequireRateLimiting(CommonRateLimitPolicyNames.Global)
			.RequireAuthorization(IdentityAuthorizationPolicyNames.BearerToken)
			.RequirePermissions(Permissions.ManageRooms)
			.HasApiVersion(1);
	}

	private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
		[FromRoute] UInt64 sourceRoomId,
		[FromRoute] UInt64 targetRoomId,
		[FromServices] ApplicationDbContext dbContext,
		[FromServices] SnowflakeGenerator snowflakeGenerator)
	{
		var sourceRoomExists = await dbContext.Rooms
			.AsNoTracking()
			.AnyAsync(r => r.Id == sourceRoomId && !r.IsRemoved);
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
			.Any(r => r.Id == targetRoomId && !r.IsRemoved);
		if (!targetRoomExists)
		{
			return TypedResults.Problem(new ProblemDetails
			{
				Title = "Invalid target room",
				Detail = "The target room does not exist.",
			});
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
