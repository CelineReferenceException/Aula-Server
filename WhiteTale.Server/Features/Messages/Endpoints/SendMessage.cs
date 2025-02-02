using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace WhiteTale.Server.Features.Messages.Endpoints;

internal sealed class SendMessage : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapPost("rooms/{roomId}/messages", HandleAsync)
			.RequireRateLimiting(RateLimitPolicyNames.Global)
			.RequireAuthorization(IdentityAuthorizationPolicyNames.BearerToken)
			.RequirePermissions(Permissions.SendMessages)
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<Ok<MessageData>, ProblemHttpResult, InternalServerError>> HandleAsync(
		[FromRoute] UInt64 roomId,
		[FromBody] SendMessageRequestBody body,
		[FromServices] SendMessageRequestBodyValidator bodyValidator,
		HttpContext httpContext,
		[FromServices] UserManager<User> userManager,
		[FromServices] ApplicationDbContext dbContext,
		[FromServices] SnowflakeGenerator snowflakeGenerator)
	{
		var validation = await bodyValidator.ValidateAsync(body);
		if (!validation.IsValid)
		{
			var problemDetails = validation.Errors.ToProblemDetails();
			return TypedResults.Problem(problemDetails);
		}

		var roomExists = await dbContext.Rooms
			.AsNoTracking()
			.AnyAsync(r => r.Id == roomId && !r.IsRemoved);
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

		var messageId = snowflakeGenerator.NewSnowflake();

		var allowedFlags = body.Type switch
		{
			MessageType.Standard => Message.StandardTypeAllowedFlags,
			MessageType.UserJoin => (MessageFlags)0,
			MessageType.UserLeave => (MessageFlags)0,
			_ => throw new UnreachableException(),
		};
		var flags = body.Flags ?? 0;
		if (flags is not 0)
		{
			flags = flags
				.GetFlags()
				.Where(flag => allowedFlags.HasFlag(flag))
				.Aggregate((x, y) => x | y);
		}

		var message = Message.Create(messageId, body.Type, flags, MessageAuthor.User, user.Id, body.Target ?? MessageTarget.Room, body.Content,
			null, null, roomId);

		_ = dbContext.Messages.Add(message);
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
			CreationTime = message.CreationTime,
		});
	}
}
