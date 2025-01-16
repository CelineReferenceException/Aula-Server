using WhiteTale.Server.Domain.Rooms;
using WhiteTale.Server.Domain.Users;

namespace WhiteTale.Server.Features.Rooms;

internal sealed class CreateRoom : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapPost("api/rooms", HandleAsync)
			.RequireRateLimiting(RateLimitPolicyNames.Global)
			.RequireAuthorization(IdentityAuthorizationPolicyNames.BearerToken)
			.RequirePermission(Permissions.ManageRooms);
	}

	private static async Task<Results<Ok<RoomData>, ProblemHttpResult>> HandleAsync(
		[FromBody] CreateRoomRequestBody body,
		[FromServices] CreateRoomRequestBodyValidator bodyValidator,
		[FromServices] ApplicationDbContext dbContext,
		[FromServices] ISnowflakeGenerator snowflakeGenerator)
	{
		var validation = await bodyValidator.ValidateAsync(body);
		if (!validation.IsValid)
		{
			var problemDetails = validation.Errors.ToProblemDetails();
			return TypedResults.Problem(problemDetails);
		}

		var room = Room.Create(snowflakeGenerator.NewSnowflake(), body.Name, body.Description, body.IsEntrance);

		_ = dbContext.Rooms.Add(room);
		_ = await dbContext.SaveChangesAsync();

		return TypedResults.Ok(new RoomData
		{
			Id = room.Id,
			Name = room.Name,
			Description = room.Description,
			IsEntrance = room.IsEntrance,
		});
	}
}
