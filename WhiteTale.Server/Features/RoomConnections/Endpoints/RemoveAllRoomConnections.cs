using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace WhiteTale.Server.Features.RoomConnections.Endpoints;

internal sealed class RemoveAllRoomConnections : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapDelete("rooms/{sourceRoomId}/connections", HandleAsync)
			.RequireRateLimiting(RateLimitPolicyNames.Global)
			.RequireAuthorization(IdentityAuthorizationPolicyNames.BearerToken)
			.RequirePermissions(Permissions.ManageRooms)
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<Ok<IEnumerable<UInt64>>, ProblemHttpResult>> HandleAsync(
		[FromRoute] UInt64 sourceRoomId,
		[FromServices] ApplicationDbContext dbContext,
		[FromServices] SnowflakeGenerator snowflakeGenerator)
	{
		var sourceRoomExists = await dbContext.Rooms
			.AsNoTracking()
			.AnyAsync(r => r.Id == sourceRoomId && !r.IsRemoved);
		if (!sourceRoomExists)
		{
			return TypedResults.Problem(ProblemDetailsDefaults.RoomDoesNotExist);
		}

		var connections = await dbContext.RoomConnections
			.AsTracking()
			.Where(c => c.SourceRoomId == sourceRoomId)
			.ToListAsync();

		connections.ForEach(c => c.Remove());
		dbContext.RoomConnections.RemoveRange(connections);
		_ = await dbContext.SaveChangesAsync();

		return TypedResults.Ok(connections.Select(c => c.TargetRoomId));
	}
}
