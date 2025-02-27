using Aula.Server.Features.Messages.Gateway;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Features.Rooms.Endpoints;

internal sealed class StartTyping : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapPost("rooms/{roomId}/start-typing", HandleAsync)
			.RequireAuthenticatedUser()
			.RequirePermissions(Permissions.SendMessages)
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<NoContent, ProblemHttpResult, InternalServerError>> HandleAsync(
		[FromRoute] UInt64 roomId,
		[FromServices] UserManager userManager,
		[FromServices] ApplicationDbContext dbContext,
		[FromServices] IPublisher publisher,
		HttpContext httpContext)
	{
		var userId = userManager.GetUserId(httpContext.User);
		if (userId is null)
		{
			return TypedResults.InternalServerError();
		}

		var roomExists = await dbContext.Rooms
			.AsNoTracking()
			.AnyAsync(r => r.Id == roomId && !r.IsRemoved);
		if (!roomExists)
		{
			return TypedResults.Problem(new ProblemDetails
			{
				Title = "Invalid room",
				Detail = "The room does not exist.",
				Status = StatusCodes.Status400BadRequest,
			});
		}

		await publisher.Publish(new UserStartedTypingEvent
		{
			UserId = (UInt64)userId,
			RoomId = roomId,
		});

		return TypedResults.NoContent();
	}
}
