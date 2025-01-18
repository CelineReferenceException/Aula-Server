using WhiteTale.Server.Domain.Users;

namespace WhiteTale.Server.Features.Rooms;

internal sealed class ModifyRoom : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapPatch("api/rooms/{roomId}", HandleAsync)
			.RequireRateLimiting(RateLimitPolicyNames.Global)
			.RequireAuthorization(IdentityAuthorizationPolicyNames.BearerToken)
			.RequirePermission(Permissions.ManageRooms);
	}

	private static async Task<Results<Ok<RoomData>, NotFound, ProblemHttpResult, InternalServerError>> HandleAsync(
		[FromRoute] UInt64 roomId,
		[FromBody] ModifyRoomRequestBody body,
		[FromServices] ModifyRoomRequestBodyValidator bodyValidator,
		[FromServices] ApplicationDbContext dbContext)
	{
		var validation = await bodyValidator.ValidateAsync(body);
		if (!validation.IsValid)
		{
			var problemDetails = validation.Errors.ToProblemDetails();
			return TypedResults.Problem(problemDetails);
		}

		var room = await dbContext.Rooms
			.AsTracking()
			.Where(room => room.Id == roomId && !room.IsRemoved)
			.FirstOrDefaultAsync();
		if (room is null)
		{
			return TypedResults.NotFound();
		}

		room.Modify(body.Name, body.Description, body.IsEntrance);

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
