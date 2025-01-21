using WhiteTale.Server.Domain.Users;

namespace WhiteTale.Server.Features.Rooms.Messages;

internal sealed class GetMessages : IEndpoint
{
	internal const String BeforeQueryParameter = "before";
	internal const String AfterQueryParameter = "after";
	internal const String CountQueryParameter = "count";

	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapGet("api/rooms/{roomid}/messages", HandleAsync)
			.RequireRateLimiting(CommonRateLimitPolicyNames.Global)
			.RequireAuthorization(IdentityAuthorizationPolicyNames.BearerToken)
			.RequirePermission(Permissions.ReadMessages);
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

		count ??= 10;
		if (count is > 100 or < 1)
		{
			return TypedResults.Problem(new ProblemDetails
			{
				Title = "Invalid message count.",
				Detail = "The message count must be between 1 and 100.",
				Status = StatusCodes.Status400BadRequest
			});
		}

		var messages = new List<MessageData>();

		if (beforeId is not null)
		{
			var targetExists = await dbContext.Messages
				.AsNoTracking()
				.AnyAsync(message => message.Id == beforeId);

			if (!targetExists)
			{
				return TypedResults.Problem(new ProblemDetails
				{
					Title = $"Invalid '{BeforeQueryParameter}' message id.",
					Detail = $"The '{BeforeQueryParameter}' message was not found.",
					Status = StatusCodes.Status400BadRequest
				});
			}

			var messagesBefore = dbContext.Messages
				.AsNoTracking()
				.Where(message => message.Id < beforeId && !message.IsRemoved)
				.OrderByDescending(message => message.Id)
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
				.Take(count.Value);

			messages.AddRange(messagesBefore);
		}
		else if (afterId is not null)
		{
			var targetExists = await dbContext.Messages
				.AsNoTracking()
				.AnyAsync(message => message.Id == afterId);

			if (!targetExists)
			{
				return TypedResults.Problem(new ProblemDetails
				{
					Title = $"Invalid '{AfterQueryParameter}' message id.",
					Detail = $"The '{AfterQueryParameter}' message was not found.",
					Status = StatusCodes.Status400BadRequest
				});
			}

			var messagesAfter = dbContext.Messages
				.AsNoTracking()
				.Where(message => message.Id > afterId && !message.IsRemoved)
				.OrderByDescending(message => message.Id)
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
				.Take(count.Value);

			messages.AddRange(messagesAfter);
		}
		else
		{
			var lastMessages = dbContext.Messages
				.AsNoTracking()
				.Where(message => !message.IsRemoved)
				.OrderByDescending(message => message.Id)
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
				.Take(count.Value);

			messages.AddRange(lastMessages);
		}

		return TypedResults.Ok(messages);
	}
}
