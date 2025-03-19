using Aula.Server.Common.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Features.Messages.Endpoints;

internal sealed class RemoveMessage : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapDelete("rooms/{roomId}/messages/{messageId}", HandleAsync)
			.ApplyRateLimiting(MessageRateLimitingPolicies.RemoveMessage)
			.RequireAuthenticatedUser()
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<NoContent, ForbidHttpResult, ProblemHttpResult, InternalServerError>> HandleAsync(
		[FromRoute] UInt64 roomId,
		[FromRoute] UInt64 messageId,
		[FromServices] ApplicationDbContext dbContext,
		HttpContext httpContext,
		[FromServices] UserManager userManager)
	{
		if (!await dbContext.Rooms.AnyAsync(r => r.Id == roomId && !r.IsRemoved))
		{
			return TypedResults.Problem(ProblemDetailsDefaults.RoomDoesNotExist);
		}

		var message = await dbContext.Messages
			.AsTracking()
			.Where(m => m.Id == messageId && !m.IsRemoved)
			.FirstOrDefaultAsync();
		if (message is null)
		{
			return TypedResults.NoContent();
		}

		var user = await userManager.GetUserAsync(httpContext.User);
		if (user is null)
		{
			return TypedResults.InternalServerError();
		}

		if (message.AuthorId != user.Id &&
		    !(user.Permissions.HasFlag(Permissions.Administrator) ||
		      user.Permissions.HasFlag(Permissions.SendMessages)))
		{
			return TypedResults.Forbid();
		}

		message.Remove();
		_ = await dbContext.SaveChangesAsync();

		return TypedResults.NoContent();
	}
}
