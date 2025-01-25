using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace WhiteTale.Server.Features.Users.CurrentRoom;

internal sealed class SetCurrentRoom : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapPut("users/{userId}/current-room/", HandleAsync)
			.RequireRateLimiting(CommonRateLimitPolicyNames.Global)
			.RequireAuthorization(IdentityAuthorizationPolicyNames.BearerToken)
			.RequirePermission(Permissions.SetCurrentRoom)
			.HasApiVersion(1);
	}

	private static async Task<Results<NoContent, ProblemHttpResult, InternalServerError>> HandleAsync(
		[FromRoute] UInt64 userId,
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

		var user = await dbContext.Users
			.AsTracking()
			.Where(u => u.Id == userId)
			.FirstOrDefaultAsync();
		if (user is null)
		{
			return TypedResults.Problem(new ProblemDetails
			{
				Title = "Invalid user",
				Detail = "The specified user does not exist.",
				Status = StatusCodes.Status400BadRequest,
			});
		}

		var room = await dbContext.Rooms
			.AsNoTracking()
			.Where(r => r.Id == body.RoomId && !r.IsRemoved)
			.Select(r => new
			{
				r.IsEntrance,
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
				Detail = "The user is in no room and the specified room is not an entrance.",
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
