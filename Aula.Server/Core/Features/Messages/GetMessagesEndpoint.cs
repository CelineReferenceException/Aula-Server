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

internal sealed class GetMessagesEndpoint : IEndpoint
{
	internal const String BeforeQueryParameter = "before";
	internal const String AfterQueryParameter = "after";
	internal const String CountQueryParameter = "count";
	internal const Int32 MinimumMessageCount = 2;
	internal const Int32 MaximumMessageCount = 100;
	private const Int32 DefaultMessageCount = 10;

	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapGet("rooms/{roomId}/messages", HandleAsync)
			.RequireAuthenticatedUser()
			.RequirePermissions(Permissions.ReadMessages)
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<Ok<IEnumerable<MessageData>>, ProblemHttpResult, InternalServerError>> HandleAsync(
		[FromRoute] Snowflake roomId,
		[FromQuery(Name = BeforeQueryParameter)] Snowflake? beforeId,
		[FromQuery(Name = AfterQueryParameter)] Snowflake? afterId,
		[FromQuery(Name = CountQueryParameter)] Int32? count,
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

		if (user.CurrentRoomId != roomId &&
		    !user.Permissions.HasFlag(Permissions.Administrator))
		{
			return TypedResults.Problem(ProblemDetailsDefaults.UserIsNotInTheRoomAndNoAdministrator);
		}

		count ??= DefaultMessageCount;
		if (count is > MaximumMessageCount or < MinimumMessageCount)
		{
			return TypedResults.Problem(ProblemDetailsDefaults.InvalidMessageCount);
		}

		var messagesQuery = dbContext.Messages
			.Where(m => !m.IsRemoved && m.RoomId == roomId)
			.OrderByDescending(m => m.CreationDate)
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
			.Take(count.Value);

		if (beforeId is not null)
		{
			var target = await dbContext.Messages
				.Where(m => m.Id == beforeId && !m.IsRemoved && m.RoomId == roomId)
				.Select(m => new
				{
					CreationTime = m.CreationDate,
				})
				.FirstOrDefaultAsync();

			if (target is null)
			{
				return TypedResults.Problem(ProblemDetailsDefaults.InvalidBeforeMessage);
			}

			messagesQuery = messagesQuery.Where(m => m.CreationTime < target.CreationTime);
		}
		else if (afterId is not null)
		{
			var target = await dbContext.Messages
				.Where(m => m.Id == afterId && !m.IsRemoved && m.RoomId == roomId)
				.Select(m => new
				{
					CreationTime = m.CreationDate,
				})
				.FirstOrDefaultAsync();

			if (target is null)
			{
				return TypedResults.Problem(ProblemDetailsDefaults.InvalidAfterMessage);
			}

			messagesQuery = messagesQuery.Where(m => m.CreationTime > target.CreationTime);
		}

		var messages = (await messagesQuery.ToListAsync()).Select(m => new MessageData
		{
			Id = m.Id,
			Type = m.Type,
			Flags = m.Flags,
			AuthorType = m.AuthorType,
			AuthorId = m.AuthorId,
			RoomId = m.RoomId,
			Content = m.Content,
			JoinData = m.JoinData is not null
				? new MessageUserJoinData
				{
					UserId = m.JoinData.UserId,
				}
				: null,
			LeaveData = m.LeaveData is not null
				? new MessageUserLeaveData
				{
					UserId = m.LeaveData.UserId,
					RoomId = m.LeaveData.RoomId,
				}
				: null,
			CreationDate = m.CreationTime,
		});

		return TypedResults.Ok(messages);
	}
}
