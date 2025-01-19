using WhiteTale.Server.Domain.Users;

namespace WhiteTale.Server.Features.Rooms;

internal sealed class RemoveRoom : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapDelete("api/rooms/{roomId}", HandleAsync)
			.RequireRateLimiting(CommonRateLimitPolicyNames.Global)
			.RequireAuthorization(IdentityAuthorizationPolicyNames.BearerToken)
			.RequirePermission(Permissions.ManageRooms);
	}

	private static async Task<Results<Ok<RoomData>, NotFound, InternalServerError>> HandleAsync(
		[FromRoute] UInt64 roomId,
		[FromServices] ApplicationDbContext dbContext)
	{
		var room = await dbContext.Rooms
			.AsTracking()
			.Where(room => room.Id == roomId && !room.IsRemoved)
			.FirstOrDefaultAsync();
		if (room is null)
		{
			return TypedResults.NotFound();
		}

		room.Remove();

		try
		{
			_ = await dbContext.SaveChangesAsync();
		}
		catch (DbUpdateConcurrencyException)
		{
			return TypedResults.InternalServerError();
		}

		return TypedResults.Ok(new RoomData
		{
			Id = room.Id,
			Name = room.Name,
			Description = room.Description,
			IsEntrance = room.IsEntrance
		});
	}
}
