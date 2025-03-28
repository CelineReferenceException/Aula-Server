using Aula.Server.Common.Authorization;
using Aula.Server.Common.Identity;
using Aula.Server.Common.Persistence;
using Aula.Server.Domain;
using Aula.Server.Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Core.Api.Rooms;

internal sealed class StopTypingApiEndpoint : IApiEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapPost("rooms/{roomId}/stop-typing", HandleAsync)
			.RequireAuthenticatedUser()
			.RequirePermissions(Permissions.SendMessages)
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<NoContent, ProblemHttpResult, InternalServerError>> HandleAsync(
		[FromRoute] Snowflake roomId,
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

		if (!await dbContext.Rooms.AnyAsync(r => r.Id == roomId && !r.IsRemoved))
		{
			return TypedResults.Problem(new ProblemDetails
			{
				Title = "Invalid room",
				Detail = "The room does not exist.",
				Status = StatusCodes.Status400BadRequest,
			});
		}

		await publisher.Publish(new UserStoppedTypingEvent
		{
			UserId = (Snowflake)userId,
			RoomId = roomId,
		});

		return TypedResults.NoContent();
	}
}
