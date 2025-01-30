using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using WhiteTale.Server.Features.Gateway.Events.Messages;

namespace WhiteTale.Server.Features.Rooms;

internal sealed class StopTyping : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapDelete("rooms/{roomId}/stop-typing", HandleAsync)
			.RequireRateLimiting(CommonRateLimitPolicyNames.Global)
			.RequireAuthorization(IdentityAuthorizationPolicyNames.BearerToken)
			.RequirePermissions(Permissions.SendMessages)
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<NoContent, ProblemHttpResult, InternalServerError>> HandleAsync(
		[FromRoute] UInt64 roomId,
		[FromServices] UserManager<User> userManager,
		[FromServices] ApplicationDbContext dbContext,
		[FromServices] IPublisher publisher,
		HttpContext httpContext)
	{
		var userIdClaimValue = userManager.GetUserId(httpContext.User);
		if (!UInt64.TryParse(userIdClaimValue, out var userId))
		{
			return TypedResults.InternalServerError();
		}

		var roomExists = await dbContext.Rooms
			.AsNoTracking()
			.AnyAsync(r => r.Id == roomId && !r.IsRemoved);
		if (!roomExists)
		{
			return TypedResults.Problem(new ProblemDetails
			{
				Title = "Invalid room",
				Detail = "The room does not exist.",
				Status = StatusCodes.Status400BadRequest,
			});
		}

		await publisher.Publish(new UserStoppedTypingEvent
		{
			UserId = userId,
			RoomId = roomId,
		});

		return TypedResults.NoContent();
	}
}
