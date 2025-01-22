using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace WhiteTale.Server.Features.Users.SetCurrentRoom;

internal sealed class SetOwnCurrentRoom : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapPut("api/users/@me/set-current-room/", HandleAsync)
			.RequireRateLimiting(CommonRateLimitPolicyNames.Global)
			.RequireAuthorization(IdentityAuthorizationPolicyNames.BearerToken)
			.RequirePermission(Permissions.SetOwnCurrentRoom);
	}

	private static async Task<Results<NoContent, ProblemHttpResult, InternalServerError>> HandleAsync(
		[FromBody] SetCurrentRoomRequestBody body,
		[FromServices] SetCurrentRoomRequestBodyValidator bodyValidator,
		[FromServices] ApplicationDbContext dbContext,
		[FromServices] UserManager<User> userManager,
		HttpContext httpContext)
	{
		var validation = await bodyValidator.ValidateAsync(body);
		if (!validation.IsValid)
		{
			var problemDetails = validation.Errors.ToProblemDetails();
			return TypedResults.Problem(problemDetails);
		}

		var userIdClaimValue = userManager.GetUserId(httpContext.User);
		if (!UInt64.TryParse(userIdClaimValue, out var userId))
		{
			return TypedResults.InternalServerError();
		}

		var user = await dbContext.Users
			.AsTracking()
			.Where(x => x.Id == userId)
			.FirstOrDefaultAsync();
		if (user is null)
		{
			return TypedResults.InternalServerError();
		}

		var room = await dbContext.Rooms
			.AsNoTracking()
			.Where(room => room.Id == body.RoomId)
			.Select(room => new
			{
				room.IsEntrance,
			})
			.FirstOrDefaultAsync();
		if (room is null)
		{
			return TypedResults.Problem(new ProblemDetails
			{
				Title = "Invalid room",
				Detail = "The room does not exist.",
				Status = StatusCodes.Status400BadRequest,
			});
		}

		if (user.CurrentRoomId is null &&
		    !room.IsEntrance)
		{
			return TypedResults.Problem(new ProblemDetails
			{
				Title = "Invalid room",
				Detail = "The first room to join must be an entrance.",
			});
		}

		var connectionsToTarget = dbContext.RoomConnections
			.AsNoTracking()
			.Where(connection => connection.SourceRoomId == user.CurrentRoomId && connection.TargetRoomId == body.RoomId);
		if (user.CurrentRoomId is not null &&
		    !await connectionsToTarget.AnyAsync())
		{
			return TypedResults.Problem(new ProblemDetails
			{
				Title = "No room connection",
				Detail = "There's no room connection to this room.",
				Status = StatusCodes.Status400BadRequest,
			});
		}

		user.SetCurrentRoom(body.RoomId);

		try
		{
			_ = await dbContext.SaveChangesAsync();
		}
		catch (DbUpdateConcurrencyException)
		{
			return TypedResults.InternalServerError();
		}

		return TypedResults.NoContent();
	}
}
