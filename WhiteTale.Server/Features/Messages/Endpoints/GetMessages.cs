using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace WhiteTale.Server.Features.Messages.Endpoints;

internal sealed class GetMessages : IEndpoint
{
	internal const String BeforeQueryParameter = "before";
	internal const String AfterQueryParameter = "after";
	internal const String CountQueryParameter = "count";
	internal const Int32 MinimumMessageCount = 2;
	internal const Int32 MaximumMessageCount = 100;
	private const Int32 DefaultMessageCount = 10;

	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapGet("rooms/{roomid}/messages", HandleAsync)
			.RequireRateLimiting(RateLimitPolicyNames.Global)
			.RequireAuthenticatedUser()
			.RequirePermissions(Permissions.ReadMessages)
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<Ok<IEnumerable<MessageData>>, ProblemHttpResult, InternalServerError>> HandleAsync(
		[FromRoute] UInt64 roomId,
		[FromQuery(Name = BeforeQueryParameter)] UInt64? beforeId,
		[FromQuery(Name = AfterQueryParameter)] UInt64? afterId,
		[FromQuery(Name = CountQueryParameter)] Int32? count,
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

		if (user.CurrentRoomId != roomId &&
		    !user.Permissions.HasFlag(Permissions.Administrator))
		{
			return TypedResults.Problem(ProblemDetailsDefaults.UserIsNotInTheRoom);
		}

		count ??= DefaultMessageCount;
		if (count is > MaximumMessageCount or < MinimumMessageCount)
		{
			return TypedResults.Problem(ProblemDetailsDefaults.InvalidMessageCount);
		}

		var messagesQuery = dbContext.Messages
			.AsNoTracking()
			.Where(m => !m.IsRemoved && (m.TargetId == roomId || m.TargetType == MessageTarget.AllRooms))
			.OrderByDescending(m => m.CreationTime)
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
			.Take(count.Value);

		if (beforeId is not null)
		{
			var target = await dbContext.Messages
				.AsNoTracking()
				.Where(m => m.Id == beforeId && !m.IsRemoved && (m.TargetId == roomId || m.TargetType == MessageTarget.AllRooms))
				.Select(m => new
				{
					m.CreationTime,
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
				.AsNoTracking()
				.Where(m => m.Id == afterId && !m.IsRemoved && (m.TargetId == roomId || m.TargetType == MessageTarget.AllRooms))
				.Select(m => new
				{
					m.CreationTime,
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
			TargetType = m.TargetType,
			TargetId = m.TargetId,
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
			CreationTime = m.CreationTime,
		});

		return TypedResults.Ok(messages);
	}
}
