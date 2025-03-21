using Aula.Server.Core.Authorization;
using Aula.Server.Core.Endpoints;
using Aula.Server.Core.Identity;
using Aula.Server.Core.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Core.Features.Messages;

internal sealed class GetMessageEndpoint : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapGet("rooms/{roomId}/messages/{messageId}", HandleAsync)
			.RequirePermissions(Permissions.ReadMessages)
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<Ok<MessageData>, NotFound, ProblemHttpResult, InternalServerError>> HandleAsync(
		[FromRoute] Snowflake roomId,
		[FromRoute] Snowflake messageId,
		[FromServices] ApplicationDbContext dbContext,
		[FromServices] UserManager userManager,
		HttpContext httpContext)
	{
		if (!await dbContext.Rooms.AnyAsync(room => room.Id == roomId && !room.IsRemoved))
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
			.Where(m => m.Id == messageId && m.RoomId == roomId && !m.IsRemoved)
			.Select(m => new
			{
				m.Id,
				m.Type,
				m.Flags,
				m.AuthorType,
				m.AuthorId,
				m.RoomId,
				m.Content,
				m.JoinData,
				m.LeaveData,
				CreationTime = m.CreationDate,
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
			RoomId = message.RoomId,
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
			CreationDate = message.CreationTime,
		});
	}
}
