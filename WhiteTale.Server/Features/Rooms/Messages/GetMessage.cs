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
		_ = route.MapGet("rooms/{roomId}/messages/{messageId}", HandleAsync)
			.RequireRateLimiting(CommonRateLimitPolicyNames.Global)
			.RequirePermissions(Permissions.ReadMessages)
			.HasApiVersion(1);
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
				Status = StatusCodes.Status400BadRequest,
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
				Status = StatusCodes.Status403Forbidden,
			});
		}

		var message = await dbContext.Messages
			.AsNoTracking()
			.Where(m => m.Id == messageId && !m.IsRemoved)
			.Select(m => new
			{
				m.Id,
				m.Type,
				m.Flags,
				m.AuthorType,
				m.AuthorId,
				Target = m.TargetType,
				m.TargetId,
				m.Content,
				m.JoinData,
				m.LeaveData,
				m.CreationTime,
			})
			.FirstOrDefaultAsync();
		if (message is null)
		{
			return TypedResults.NotFound();
		}

		return TypedResults.Ok(new MessageData
		{
			Id = message.Id,
			Type = message.Type,
			Flags = message.Flags,
			AuthorType = message.AuthorType,
			AuthorId = message.AuthorId,
			TargetType = message.Target,
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
