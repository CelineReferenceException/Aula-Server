using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace WhiteTale.Server.Features.Rooms.Messages;

internal sealed class GetMessages : IEndpoint
{
	internal const String BeforeQueryParameter = "before";
	internal const String AfterQueryParameter = "after";
	internal const String CountQueryParameter = "count";

	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapGet("rooms/{roomid}/messages", HandleAsync)
			.RequireRateLimiting(RateLimitPolicyNames.Global)
			.RequireAuthorization(IdentityAuthorizationPolicyNames.BearerToken)
			.RequirePermissions(Permissions.ReadMessages)
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<Ok<List<MessageData>>, ProblemHttpResult, InternalServerError>> HandleAsync(
		[FromRoute] UInt64 roomId,
		[FromQuery(Name = BeforeQueryParameter)] UInt64? beforeId,
		[FromQuery(Name = AfterQueryParameter)] UInt64? afterId,
		[FromQuery(Name = CountQueryParameter)] Int32? count,
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

		count ??= 10;
		if (count is > 100 or < 1)
		{
			return TypedResults.Problem(new ProblemDetails
			{
				Title = "Invalid message count.",
				Detail = "The message count must be between 1 and 100.",
				Status = StatusCodes.Status400BadRequest,
			});
		}

		var messages = new List<MessageData>();

		if (beforeId is not null)
		{
			var targetExists = await dbContext.Messages
				.AsNoTracking()
				.AnyAsync(m => m.Id == beforeId && !m.IsRemoved && m.TargetId == roomId);

			if (!targetExists)
			{
				return TypedResults.Problem(new ProblemDetails
				{
					Title = $"Invalid '{BeforeQueryParameter}' message id.",
					Detail = $"The '{BeforeQueryParameter}' message was not found.",
					Status = StatusCodes.Status400BadRequest,
				});
			}

			var messagesBefore = dbContext.Messages
				.AsNoTracking()
				.Where(m => m.Id < beforeId && !m.IsRemoved && m.TargetId == roomId)
				.OrderByDescending(m => m.Id)
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
				.Take(count.Value)
				.ToList()
				.Select(m => new MessageData
				{
					Id = m.Id,
					Type = m.Type,
					Flags = m.Flags,
					AuthorType = m.AuthorType,
					AuthorId = m.AuthorId,
					TargetType = m.Target,
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

			messages.AddRange(messagesBefore);
		}
		else if (afterId is not null)
		{
			var targetExists = await dbContext.Messages
				.AsNoTracking()
				.AnyAsync(m => m.Id == afterId && !m.IsRemoved && m.TargetId == roomId);

			if (!targetExists)
			{
				return TypedResults.Problem(new ProblemDetails
				{
					Title = $"Invalid '{AfterQueryParameter}' message id.",
					Detail = $"The '{AfterQueryParameter}' message was not found.",
					Status = StatusCodes.Status400BadRequest,
				});
			}

			var messagesAfter = dbContext.Messages
				.AsNoTracking()
				.Where(m => m.Id > afterId && !m.IsRemoved && m.TargetId == roomId)
				.OrderByDescending(m => m.Id)
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
				.Take(count.Value)
				.ToList()
				.Select(m => new MessageData
				{
					Id = m.Id,
					Type = m.Type,
					Flags = m.Flags,
					AuthorType = m.AuthorType,
					AuthorId = m.AuthorId,
					TargetType = m.Target,
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

			messages.AddRange(messagesAfter);
		}
		else
		{
			var lastMessages = dbContext.Messages
				.AsNoTracking()
				.Where(m => !m.IsRemoved && m.TargetId == roomId)
				.OrderByDescending(m => m.Id)
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
				.Take(count.Value)
				.ToList()
				.Select(m => new MessageData
				{
					Id = m.Id,
					Type = m.Type,
					Flags = m.Flags,
					AuthorType = m.AuthorType,
					AuthorId = m.AuthorId,
					TargetType = m.Target,
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

			messages.AddRange(lastMessages);
		}

		return TypedResults.Ok(messages);
	}
}
