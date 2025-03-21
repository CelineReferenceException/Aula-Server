﻿using Aula.Server.Core.Authorization;
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

namespace Aula.Server.Core.Features.Messages;

internal sealed class RemoveMessageEndpoint : IEndpoint
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
		[FromRoute] Snowflake roomId,
		[FromRoute] Snowflake messageId,
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
