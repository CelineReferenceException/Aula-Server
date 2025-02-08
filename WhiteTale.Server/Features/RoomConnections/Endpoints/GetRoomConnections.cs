using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace WhiteTale.Server.Features.RoomConnections.Endpoints;

internal sealed class GetRoomConnections : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapGet("rooms/{roomId}/connections", HandleAsync)
			.RequireRateLimiting(RateLimitPolicyNames.Global)
			.RequireAuthenticatedUser()
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<Ok<List<UInt64>>, ProblemHttpResult>> HandleAsync(
		[FromRoute] UInt64 roomId,
		[FromServices] ApplicationDbContext dbContext)
	{
		var roomExists = await dbContext.Rooms
			.AsNoTracking()
			.AnyAsync(r => r.Id == roomId && !r.IsRemoved);
		if (!roomExists)
		{
			return TypedResults.Problem(ProblemDetailsDefaults.RoomDoesNotExist);
		}

		var connections = await dbContext.RoomConnections
			.Where(c => c.SourceRoomId == roomId)
			.Select(c => c.TargetRoomId)
			.ToListAsync();

		return TypedResults.Ok(connections);
	}
}
