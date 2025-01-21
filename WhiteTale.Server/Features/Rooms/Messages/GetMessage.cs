using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace WhiteTale.Server.Features.Rooms.Messages;

internal sealed class GetMessage : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapGet("api/rooms/{roomId}/messages/{messageId}", HandleAsync)
			.RequireRateLimiting(CommonRateLimitPolicyNames.Global)
			.RequirePermission(Permissions.ReadMessages);
	}

	private static async Task<Results<Ok<MessageData>, NotFound, ProblemHttpResult, InternalServerError>> HandleAsync(
		[FromRoute] UInt64 roomId,
		[FromRoute] UInt64 messageId,
		[FromServices] ApplicationDbContext dbContext,
		[FromServices] UserManager<User> userManager,
		HttpContext httpContext)
	{
		var roomExists = await dbContext.Rooms
			.AsNoTracking()
			.AnyAsync(room => room.Id == roomId && !room.IsRemoved);
		if (!roomExists)
		{
			return TypedResults.Problem(new ProblemDetails
			{
				Title = "Invalid room ID",
				Detail = "The room does not exist",
				Status = StatusCodes.Status400BadRequest
			});
		}

		var user = await userManager.GetUserAsync(httpContext.User);
		if (user is null)
		{
			return TypedResults.InternalServerError();
		}

		if (user.CurrentRoomId != roomId)
		{
			return TypedResults.Problem(new ProblemDetails
			{
				Title = "Invalid room",
				Detail = "The user is not in the room",
				Status = StatusCodes.Status403Forbidden
			});
		}

		var message = await dbContext.Messages
			.AsNoTracking()
			.Where(message => message.Id == messageId && !message.IsRemoved)
			.Select(message => new MessageData
			{
				Id = message.Id,
				Type = message.Type,
				Flags = message.Flags,
				AuthorId = message.AuthorId,
				Target = message.Target,
				TargetId = message.TargetId,
				Content = message.Content,
				CreationTime = message.CreationTime
			})
			.FirstOrDefaultAsync();
		if (message is null)
		{
			return TypedResults.NotFound();
		}

		return TypedResults.Ok(message);
	}
}
