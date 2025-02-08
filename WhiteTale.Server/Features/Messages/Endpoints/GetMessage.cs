using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace WhiteTale.Server.Features.Messages.Endpoints;

internal sealed class GetMessage : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapGet("rooms/{roomId}/messages/{messageId}", HandleAsync)
			.RequireRateLimiting(RateLimitPolicyNames.Global)
			.RequirePermissions(Permissions.ReadMessages)
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<Ok<MessageData>, NotFound, ProblemHttpResult, InternalServerError>> HandleAsync(
		[FromRoute] UInt64 roomId,
		[FromRoute] UInt64 messageId,
		[FromServices] ApplicationDbContext dbContext,
		[FromServices] UserManager userManager,
		HttpContext httpContext)
	{
		var roomExists = await dbContext.Rooms
			.AsNoTracking()
			.AnyAsync(room => room.Id == roomId && !room.IsRemoved);
		if (!roomExists)
		{
			return TypedResults.Problem(ProblemDetailsDefaults.RoomDoesNotExist);
		}

		var user = await userManager.GetUserAsync(httpContext.User);
		if (user is null)
		{
			return TypedResults.InternalServerError();
		}

		if (user.CurrentRoomId != roomId)
		{
			return TypedResults.Problem(ProblemDetailsDefaults.UserIsNotInTheRoom);
		}

		var message = await dbContext.Messages
			.AsNoTracking()
			.Where(m => m.Id == messageId && (m.TargetId == roomId || m.TargetType == MessageTarget.AllRooms) && !m.IsRemoved)
			.Select(m => new
			{
				m.Id,
				m.Type,
				m.Flags,
				m.AuthorType,
				m.AuthorId,
				m.TargetType,
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
