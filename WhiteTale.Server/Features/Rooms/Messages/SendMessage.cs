using System.Diagnostics;
using WhiteTale.Server.Domain.Messages;
using WhiteTale.Server.Domain.Users;

namespace WhiteTale.Server.Features.Rooms.Messages;

internal sealed class SendMessage : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapPost("api/rooms/{roomId}/messages", HandleAsync)
			.RequireRateLimiting(CommonRateLimitPolicyNames.Global)
			.RequireAuthorization(IdentityAuthorizationPolicyNames.BearerToken)
			.RequirePermission(Permissions.SendMessages);
	}

	private static async Task<Results<Ok<MessageData>, ProblemHttpResult, InternalServerError>> HandleAsync(
		[FromRoute] UInt64 roomId,
		[FromBody] SendMessageRequestBody body,
		[FromServices] SendMessageRequestBodyValidator bodyValidator,
		HttpContext httpContext,
		[FromServices] UserManager<User> userManager,
		[FromServices] ApplicationDbContext dbContext,
		[FromServices] ISnowflakeGenerator snowflakeGenerator)
	{
		var validation = await bodyValidator.ValidateAsync(body);
		if (!validation.IsValid)
		{
			var problemDetails = validation.Errors.ToProblemDetails();
			return TypedResults.Problem(problemDetails);
		}

		var roomExists = await dbContext.Rooms
			.AsNoTracking()
			.AnyAsync(room => room.Id == roomId && !room.IsRemoved);
		if (!roomExists)
		{
			return TypedResults.Problem(new ProblemDetails
			{
				Title = "Invalid room",
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

		var messageId = snowflakeGenerator.NewSnowflake();

		var allowedFlags = body.Type switch
		{
			MessageType.Standard => Message.StandardTypeAllowedFlags,
			_ => throw new UnreachableException()
		};
		var flags = body.Flags ?? 0;
		if (flags is not 0)
		{
			flags = flags
				.GetFlags()
				.Where(flag => allowedFlags.HasFlag(flag))
				.Aggregate((x, y) => x | y);
		}

		var message = Message.Create(messageId, body.Type, flags, user.Id, body.Target ?? MessageTarget.Room, body.Content,
			roomId);

		_ = dbContext.Messages.Add(message);
		_ = await dbContext.SaveChangesAsync();

		return TypedResults.Ok(new MessageData
		{
			Id = message.Id,
			Type = message.Type,
			Flags = message.Flags,
			AuthorId = message.AuthorId,
			Target = message.Target,
			TargetId = message.TargetId,
			Content = message.Content,
			CreationTime = message.CreationTime
		});
	}
}
