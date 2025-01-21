using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace WhiteTale.Server.Features.Rooms.Connections;

internal sealed class RemoveAllConnections : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapDelete("api/rooms/{sourceRoomId}/connections", HandleAsync)
			.RequireRateLimiting(CommonRateLimitPolicyNames.Global)
			.RequireAuthorization(IdentityAuthorizationPolicyNames.BearerToken)
			.RequirePermission(Permissions.ManageRooms);
	}

	private static async Task<Results<Ok<IEnumerable<UInt64>>, ProblemHttpResult>> HandleAsync(
		[FromRoute] UInt64 sourceRoomId,
		[FromServices] ApplicationDbContext dbContext,
		[FromServices] SnowflakeGenerator snowflakeGenerator)
	{
		var sourceRoomExists = await dbContext.Rooms
			.AsNoTracking()
			.AnyAsync(room => room.Id == sourceRoomId);
		if (!sourceRoomExists)
		{
			return TypedResults.Problem(new ProblemDetails
			{
				Title = "Invalid room",
				Detail = "The room does not exist.",
			});
		}

		var connections = await dbContext.RoomConnections
			.AsTracking()
			.Where(connection => connection.SourceRoomId == sourceRoomId)
			.ToListAsync();

		connections.ForEach(connection => connection.Remove());
		dbContext.RoomConnections.RemoveRange(connections);
		_ = await dbContext.SaveChangesAsync();

		return TypedResults.Ok(connections.Select(connection => connection.TargetRoomId));
	}
}
