using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace WhiteTale.Server.Features.Rooms;

internal sealed class GetAllRooms : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapGet("api/rooms", HandleAsync)
			.RequireRateLimiting(CommonRateLimitPolicyNames.Global)
			.RequireAuthorization(IdentityAuthorizationPolicyNames.BearerToken);
	}

	private static async Task<Ok<List<RoomData>>> HandleAsync([FromServices] ApplicationDbContext dbContext)
	{
		var rooms = await dbContext.Rooms
			.AsNoTracking()
			.Where(room => !room.IsRemoved)
			.Select(room =>
				new RoomData
				{
					Id = room.Id,
					Name = room.Name,
					Description = room.Description,
					IsEntrance = room.IsEntrance
				})
			.ToListAsync();

		return TypedResults.Ok(rooms);
	}
}
