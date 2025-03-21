using Aula.Server.Core.Authorization;
using Aula.Server.Core.Endpoints;
using Aula.Server.Core.Identity;
using Aula.Server.Core.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Core.Features.Bots;

internal sealed class ResetBotTokenEndpoint : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapPost("bots/{userId}/reset-token", HandleAsync)
			.RequireAuthenticatedUser()
			.RequireUserType(UserType.Standard)
			.RequirePermissions()
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<Ok<ResetBotTokenResponse>, ProblemHttpResult, InternalServerError>> HandleAsync(
		[FromRoute] Snowflake userId,
		[FromServices] ApplicationDbContext dbContext,
		[FromServices] TokenProvider tokenProvider)
	{
		var user = await dbContext.Users
			.AsTracking()
			.Where(u => u.Id == userId && u.Type == UserType.Bot && !u.IsRemoved)
			.FirstOrDefaultAsync();
		if (user is null)
		{
			return TypedResults.Problem(ProblemDetailsDefaults.UserDoesNotExist);
		}

		user.UpdateSecurityStamp();

		try
		{
			_ = await dbContext.SaveChangesAsync();
		}
		catch (DbUpdateConcurrencyException)
		{
			return TypedResults.InternalServerError();
		}

		return TypedResults.Ok(new ResetBotTokenResponse
		{
			Token = tokenProvider.CreateToken(user),
		});
	}
}
