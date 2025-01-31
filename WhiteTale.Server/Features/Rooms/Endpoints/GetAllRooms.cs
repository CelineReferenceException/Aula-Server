using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace WhiteTale.Server.Features.Rooms.Endpoints;

internal sealed class GetAllRooms : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapGet("rooms", HandleAsync)
			.RequireRateLimiting(RateLimitPolicyNames.Global)
			.RequireAuthorization(IdentityAuthorizationPolicyNames.BearerToken)
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Ok<List<RoomData>>> HandleAsync([FromServices] ApplicationDbContext dbContext)
	{
		var rooms = await dbContext.Rooms
			.AsNoTracking()
			.Where(r => !r.IsRemoved)
			.Select(r =>
				new RoomData
				{
					Id = r.Id,
					Name = r.Name,
					Description = r.Description,
					IsEntrance = r.IsEntrance,
					CreationTime = r.CreationTime,
				})
			.ToListAsync();

		return TypedResults.Ok(rooms);
	}
}
