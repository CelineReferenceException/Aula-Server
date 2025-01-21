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

		var roomExists = await dbContext.Rooms
			.AsNoTracking()
			.AnyAsync(room => room.Id == body.RoomId);
		if (!roomExists)
		{
			return TypedResults.Problem(new ProblemDetails
			{
				Title = "Invalid room",
				Detail = "The room does not exist.",
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
