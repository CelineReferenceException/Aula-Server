using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace WhiteTale.Server.Features.Rooms;

internal sealed class GetRoom : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapGet("api/rooms/{roomId}", HandleAsync)
			.RequireRateLimiting(CommonRateLimitPolicyNames.Global)
			.RequireAuthorization(IdentityAuthorizationPolicyNames.BearerToken);
	}

	private static async Task<Results<Ok<RoomData>, NotFound>> HandleAsync(
		[FromRoute] UInt64 roomId,
		[FromServices] ApplicationDbContext dbContext)
	{
		var room = await dbContext.Rooms
			.AsNoTracking()
			.Where(room => room.Id == roomId && !room.IsRemoved)
			.Select(room =>
				new RoomData
				{
					Id = room.Id,
					Name = room.Name,
					Description = room.Description,
					IsEntrance = room.IsEntrance,
				})
			.FirstOrDefaultAsync();
		if (room is null)
		{
			return TypedResults.NotFound();
		}

		return TypedResults.Ok(room);
	}
}
