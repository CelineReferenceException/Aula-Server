using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Features.Rooms.Endpoints;

internal sealed class AddRoomConnection : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapPost("rooms/{roomId}/connections/{targetId}", HandleAsync)
			.RequireRateLimiting(RateLimitPolicyNames.Global)
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

		var sourceRoomExists = await dbContext.Rooms
			.AsNoTracking()
			.AnyAsync(r => r.Id == roomId && !r.IsRemoved);
		if (!sourceRoomExists)
		{
			return TypedResults.Problem(ProblemDetailsDefaults.RoomDoesNotExist);
		}

		var targetRoomExists = dbContext.Rooms
			.AsNoTracking()
			.Any(r => r.Id == targetId && !r.IsRemoved);
		if (!targetRoomExists)
		{
			return TypedResults.Problem(ProblemDetailsDefaults.TargetRoomDoesNotExist);
		}

		var roomConnection = RoomConnection.Create(snowflakeGenerator.NewSnowflake(), roomId, targetId);

		_ = await dbContext.AddAsync(roomConnection);
		_ = await dbContext.SaveChangesAsync();

		return TypedResults.NoContent();
	}
}
