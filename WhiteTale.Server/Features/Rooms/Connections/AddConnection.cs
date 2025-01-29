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
		_ = route.MapPut("rooms/{roomId}/connections", HandleAsync)
			.RequireRateLimiting(CommonRateLimitPolicyNames.Global)
			.RequireAuthorization(IdentityAuthorizationPolicyNames.BearerToken)
			.RequirePermissions(Permissions.ManageRooms)
			.HasApiVersion(1);
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

		if (roomId == body.RoomId)
		{
			return TypedResults.Problem(new ProblemDetails
			{
				Title = "Invalid target room",
				Detail = "The target room cannot be the same as the source room.",
				Status = StatusCodes.Status400BadRequest,
			});
		}

		var sourceRoomExists = await dbContext.Rooms
			.AsNoTracking()
			.AnyAsync(r => r.Id == roomId && !r.IsRemoved);
		if (!sourceRoomExists)
		{
			return TypedResults.Problem(new ProblemDetails
			{
				Title = "Invalid room",
				Detail = "The room does not exist.",
				Status = StatusCodes.Status400BadRequest,
			});
		}

		var targetRoomExists = dbContext.Rooms
			.AsNoTracking()
			.Any(r => r.Id == body.RoomId && !r.IsRemoved);
		if (!targetRoomExists)
		{
			return TypedResults.Problem(new ProblemDetails
			{
				Title = "Invalid target room",
				Detail = "The target room does not exist.",
				Status = StatusCodes.Status400BadRequest,
			});
		}

		var roomConnection = RoomConnection.Create(snowflakeGenerator.NewSnowflake(), roomId, body.RoomId);

		_ = await dbContext.AddAsync(roomConnection);
		_ = await dbContext.SaveChangesAsync();

		return TypedResults.NoContent();
	}
}
