using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace WhiteTale.Server.Features.Rooms.Endpoints;

internal sealed class GetRooms : IEndpoint
{
	private const String CountQueryParameter = "count";
	internal const String AfterQueryParameter = "after";
	internal const Int32 MinimumRoomCount = 2;
	internal const Int32 MaximumRoomCount = 100;
	private const Int32 DefaultRoomCount = 10;

	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapGet("rooms", HandleAsync)
			.RequireRateLimiting(RateLimitPolicyNames.Global)
			.RequireAuthenticatedUser()
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<Ok<List<RoomData>>, ProblemHttpResult>> HandleAsync(
		[FromQuery(Name = CountQueryParameter)] Int32? count,
		[FromQuery(Name = AfterQueryParameter)] UInt64? afterId,
		[FromServices] ApplicationDbContext dbContext)
	{
		count ??= DefaultRoomCount;
		if (count is > MaximumRoomCount or < MinimumRoomCount)
		{
			return TypedResults.Problem(ProblemDetailsDefaults.InvalidRoomCount);
		}

		var roomsQuery = dbContext.Rooms
			.AsNoTracking()
			.Where(r => !r.IsRemoved)
			.OrderBy(r => r.CreationTime)
			.Select(r => new RoomData
			{
				Id = r.Id,
				Name = r.Name,
				Description = r.Description,
				IsEntrance = r.IsEntrance,
				ConnectedRoomIds = r.Connections.Select(c => c.TargetRoomId),
				CreationTime = r.CreationTime,
			})
			.Take((Int32)count);

		if (afterId is not null)
		{
			var target = await dbContext.Rooms
				.AsNoTracking()
				.Where(r => r.Id == afterId && !r.IsRemoved)
				.Select(r => new
				{
					r.CreationTime,
				})
				.FirstOrDefaultAsync();

			if (target is null)
			{
				return TypedResults.Problem(ProblemDetailsDefaults.InvalidAfterRoom);
			}

			roomsQuery = roomsQuery.Where(r => r.CreationTime > target.CreationTime);
		}

		var rooms = await roomsQuery.ToListAsync();
		return TypedResults.Ok(rooms);
	}
}
