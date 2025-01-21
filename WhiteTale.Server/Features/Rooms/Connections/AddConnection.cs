using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace WhiteTale.Server.Features.Rooms.Connections;

internal sealed class AddConnection : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapPut("api/rooms/{roomId}/connections", HandleAsync)
			.RequireRateLimiting(CommonRateLimitPolicyNames.Global)
			.RequireAuthorization(IdentityAuthorizationPolicyNames.BearerToken)
			.RequirePermission(Permissions.ManageRooms);
	}

	private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
		[FromRoute] UInt64 roomId,
		[FromBody] AddConnectionRequestBody body,
		[FromServices] AddConnectionRequestBodyValidator bodyValidator,
		[FromServices] ApplicationDbContext dbContext,
		[FromServices] SnowflakeGenerator snowflakeGenerator)
	{
		var validation = await bodyValidator.ValidateAsync(body);
		if (!validation.IsValid)
		{
			var problemDetails = validation.Errors.ToProblemDetails();
			return TypedResults.Problem(problemDetails);
		}

		var sourceRoomExists = await dbContext.Rooms
			.AsNoTracking()
			.AnyAsync(room => room.Id == roomId);
		if (!sourceRoomExists)
		{
			return TypedResults.Problem(new ProblemDetails
			{
				Title = "Invalid room",
				Detail = "The room does not exist.",
			});
		}

		var targetRoomExists = dbContext.Rooms
			.AsNoTracking()
			.Any(room => room.Id == body.TargetId);
		if (!targetRoomExists)
		{
			return TypedResults.Problem(new ProblemDetails
			{
				Title = "Invalid target room",
				Detail = "The target room does not exist.",
			});
		}

		var roomConnection = RoomConnection.Create(snowflakeGenerator.NewSnowflake(), roomId, body.TargetId);

		_ = await dbContext.AddAsync(roomConnection);
		_ = await dbContext.SaveChangesAsync();

		return TypedResults.NoContent();
	}
}
