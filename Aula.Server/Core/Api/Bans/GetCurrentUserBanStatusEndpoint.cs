﻿using Aula.Server.Common.Authorization;
using Aula.Server.Common.Identity;
using Aula.Server.Common.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Core.Api.Bans;

internal sealed class GetCurrentUserBanStatusEndpoint : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapGet("bans/@me", HandleAsync)
			.RequireAuthenticatedUser()
			.HasApiVersion(1);
	}

	private static async Task<Results<Ok<GetCurrentUserBanStatusResponseBody>, InternalServerError>> HandleAsync(
		HttpContext httpContext,
		[FromServices] UserManager userManager,
		[FromServices] ApplicationDbContext dbContext)
	{
		var userId = userManager.GetUserId(httpContext.User);
		if (userId is null)
		{
			return TypedResults.InternalServerError();
		}

		return TypedResults.Ok(new GetCurrentUserBanStatusResponseBody
		{
			Banned = await dbContext.Bans.AnyAsync(x => x.TargetId == userId),
		});
	}
}
