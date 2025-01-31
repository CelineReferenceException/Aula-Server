using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace WhiteTale.Server.Features.Messages.Endpoints;

internal sealed class RemoveMessage : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapDelete("rooms/{roomId}/messages/{messageId}", HandleAsync)
			.RequireRateLimiting(RateLimitPolicyNames.Global)
			.RequireAuthorization(IdentityAuthorizationPolicyNames.BearerToken)
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<Ok<MessageData>, NotFound, ForbidHttpResult, ProblemHttpResult, InternalServerError>> HandleAsync(
		[FromRoute] UInt64 roomId,
		[FromRoute] UInt64 messageId,
		[FromServices] ApplicationDbContext dbContext,
		HttpContext httpContext,
		[FromServices] UserManager<User> userManager)
	{
		var roomExists = await dbContext.Rooms
			.AsNoTracking()
			.AnyAsync(r => r.Id == roomId && !r.IsRemoved);
		if (!roomExists)
		{
			return TypedResults.Problem(new ProblemDetails
			{
				Title = "Invalid room ID",
				Detail = "The room does not exist",
				Status = StatusCodes.Status400BadRequest,
			});
		}

		var message = await dbContext.Messages
			.AsTracking()
			.Where(m => m.Id == messageId && !m.IsRemoved)
			.Include(m => m.JoinData)
			.Include(m => m.LeaveData)
			.FirstOrDefaultAsync();
		if (message is null)
		{
			return TypedResults.NotFound();
		}

		var user = await userManager.GetUserAsync(httpContext.User);
		if (user is null)
		{
			return TypedResults.InternalServerError();
		}

		if (message.AuthorId != user.Id &&
		    !user.Permissions.HasAnyFlag(Permissions.Administrator | Permissions.ManageMessages))
		{
			return TypedResults.Forbid();
		}

		message.Remove();
		_ = await dbContext.SaveChangesAsync();

		return TypedResults.Ok(new MessageData
		{
			Id = message.Id,
			Type = message.Type,
			Flags = message.Flags,
			AuthorType = message.AuthorType,
			AuthorId = message.AuthorId,
			TargetType = message.TargetType,
			TargetId = message.TargetId,
			Content = message.Content,
			JoinData = message.JoinData is not null
				? new MessageUserJoinData
				{
					UserId = message.JoinData.UserId,
				}
				: null,
			LeaveData = message.LeaveData is not null
				? new MessageUserLeaveData
				{
					UserId = message.LeaveData.UserId,
					RoomId = message.LeaveData.RoomId,
				}
				: null,
			CreationTime = message.CreationTime,
		});
	}
}
