using Aula.Server.Core.Authorization;
using Aula.Server.Core.Endpoints;
using Aula.Server.Core.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Core.Features.Users;

internal sealed class GetUserEndpoint : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapGet("users/{userId}", HandleAsync)
			.RequireAuthenticatedUser()
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<Ok<UserData>, NotFound>> HandleAsync(
		[FromRoute] Snowflake userId,
		[FromServices] ApplicationDbContext dbContext)
	{
		var user = await dbContext.Users
			.Where(u => u.Id == userId && !u.IsRemoved)
			.Select(u =>
				new UserData
				{
					Id = userId,
					DisplayName = u.DisplayName,
					Description = u.Description,
					CurrentRoomId = u.CurrentRoomId,
					Type = u.Type,
					Presence = u.Presence,
					Permissions = u.Permissions,
				})
			.FirstOrDefaultAsync();
		if (user is null)
		{
			return TypedResults.NotFound();
		}

		return TypedResults.Ok(user);
	}
}
