using Aula.Server.Core.Authorization;
using Aula.Server.Core.Endpoints;
using Aula.Server.Core.Identity;
using Aula.Server.Core.Persistence;
using Aula.Server.Core.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Core.Features.Messages.Endpoints;

internal sealed class SendMessage : IEndpoint
{
	private static readonly IReadOnlyList<MessageType> s_allowedMessageTypes = [MessageType.Standard,];

	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapPost("rooms/{roomId}/messages", HandleAsync)
			.ApplyRateLimiting(MessageRateLimitingPolicies.SendMessage)
			.RequireAuthenticatedUser()
			.RequirePermissions(Permissions.SendMessages)
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<Created<MessageData>, ProblemHttpResult, InternalServerError>> HandleAsync(
		[FromRoute] UInt64 roomId,
		[FromBody] SendMessageRequestBody body,
		[FromServices] SendMessageRequestBodyValidator bodyValidator,
		HttpContext httpContext,
		[FromServices] UserManager userManager,
		[FromServices] ApplicationDbContext dbContext,
		[FromServices] SnowflakeGenerator snowflakeGenerator)
	{
		var validation = await bodyValidator.ValidateAsync(body);
		if (!validation.IsValid)
		{
			var problemDetails = validation.Errors.ToProblemDetails();
			return TypedResults.Problem(problemDetails);
		}

		if (!await dbContext.Rooms.AnyAsync(r => r.Id == roomId && !r.IsRemoved))
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

		if (!s_allowedMessageTypes.Contains(body.Type))
		{
			return TypedResults.Problem(ProblemDetailsDefaults.InvalidMessageType);
		}

		var messageId = await snowflakeGenerator.NewSnowflakeAsync();
		var flags = body.Flags ?? 0;

		var message = new Message(messageId, body.Type, flags, MessageAuthorType.User, user.Id, body.Content, null, null, roomId);

		_ = dbContext.Messages.Add(message);
		_ = await dbContext.SaveChangesAsync();

		return TypedResults.Created($"{httpContext.Request.GetUrl()}/{messageId}", new MessageData
		{
			Id = message.Id,
			Type = message.Type,
			Flags = message.Flags,
			AuthorType = message.AuthorType,
			AuthorId = message.AuthorId,
			RoomId = message.RoomId,
			Content = message.Content,
			CreationDate = message.CreationDate,
		});
	}
}
