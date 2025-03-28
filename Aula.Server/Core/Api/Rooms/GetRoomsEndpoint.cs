using Aula.Server.Common.Authorization;
using Aula.Server.Common.Persistence;
using Aula.Server.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Core.Api.Rooms;

internal sealed class GetRoomsEndpoint : IEndpoint
{
	internal const String CountQueryParameter = "count";
	internal const String AfterQueryParameter = "after";
	internal const Int32 MinimumRoomCount = 1;
	internal const Int32 MaximumRoomCount = 100;
	internal const Int32 DefaultRoomCount = 10;

	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapGet("rooms", HandleAsync)
			.RequireAuthenticatedUser()
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<Ok<List<RoomData>>, ProblemHttpResult>> HandleAsync(
		[FromQuery(Name = CountQueryParameter)] Int32? count,
		[FromQuery(Name = AfterQueryParameter)] Snowflake? afterId,
		[FromServices] ApplicationDbContext dbContext)
	{
		count ??= DefaultRoomCount;
		if (count is > MaximumRoomCount or < MinimumRoomCount)
		{
			return TypedResults.Problem(ProblemDetailsDefaults.InvalidRoomCount);
		}

		var roomsQuery = dbContext.Rooms
			.Where(r => !r.IsRemoved)
			.OrderBy(r => r.CreationDate)
			.Select(r => new RoomData
			{
				Id = r.Id,
				Name = r.Name,
				Description = r.Description,
				IsEntrance = r.IsEntrance,
				ConnectedRoomIds = r.Connections.Select(c => c.TargetRoomId).ToArray(),
				CreationDate = r.CreationDate,
			})
			.Take((Int32)count);

		if (afterId is not null)
		{
			var target = await dbContext.Rooms
				.Where(r => r.Id == afterId && !r.IsRemoved)
				.Select(r => new
				{
					CreationTime = r.CreationDate,
				})
				.FirstOrDefaultAsync();

			if (target is null)
			{
				return TypedResults.Problem(ProblemDetailsDefaults.InvalidAfterRoom);
			}

			roomsQuery = roomsQuery.Where(r => r.CreationDate > target.CreationTime);
		}

		var rooms = await roomsQuery.ToListAsync();
		return TypedResults.Ok(rooms);
	}
}
