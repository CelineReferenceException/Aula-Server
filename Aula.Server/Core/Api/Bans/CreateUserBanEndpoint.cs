﻿using Aula.Server.Common.Authorization;
using Aula.Server.Common.Identity;
using Aula.Server.Common.Persistence;
using Aula.Server.Domain;
using Aula.Server.Domain.Bans;
using Aula.Server.Domain.Users;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Core.Api.Bans;

internal sealed class CreateUserBanEndpoint : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapPut("bans/users/{userId}", HandleAsync)
			.RequireAuthenticatedUser()
			.RequirePermissions(Permissions.BanUsers)
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<Created<BanData>, ProblemHttpResult, InternalServerError>> HandleAsync(
		[FromRoute] Snowflake userId,
		[FromBody] CreateUserBanRequestBody body,
		[FromServices] IValidator<CreateUserBanRequestBody> bodyValidator,
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

		var currentUserId = userManager.GetUserId(httpContext.User);
		if (currentUserId is null)
		{
			return TypedResults.InternalServerError();
		}

		if (await dbContext.Bans.AnyAsync(b => b.TargetId == userId))
		{
			return TypedResults.Problem(ProblemDetailsDefaults.UserAlreadyBanned);
		}

		var targetUser = await dbContext.Users
			.Where(u => u.Id == userId)
			.Select(u => new
			{
				u.Permissions,
			})
			.FirstOrDefaultAsync();
		if (targetUser is null)
		{
			return TypedResults.Problem(ProblemDetailsDefaults.TargetDoesNotExist);
		}

		if (targetUser.Permissions.HasFlag(Permissions.Administrator))
		{
			return TypedResults.Problem(ProblemDetailsDefaults.TargetIsAdministrator);
		}

		var ban = Ban.Create(await snowflakeGenerator.NewSnowflakeAsync(), BanType.Id, currentUserId, body.Reason, userId).Value!;
		_ = dbContext.Bans.Add(ban);

		_ = await dbContext.SaveChangesAsync();

		return TypedResults.Created(httpContext.Request.GetUrl(), new BanData
		{
			Type = ban.Type,
			ExecutorId = ban.ExecutorId,
			Reason = ban.Reason,
			TargetId = ban.TargetId,
			CreationDate = ban.CreationDate,
		});
	}
}
