using Aula.Server.Common.Authorization;
using Aula.Server.Common.Endpoints;
using Aula.Server.Common.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Aula.Server.Core.Api.Users;

internal sealed class GetCurrentUserEndpoint : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapGet("users/@me", HandleAsync)
			.RequireAuthenticatedUser()
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<Ok<UserData>, InternalServerError>> HandleAsync(
		HttpContext httpContext,
		[FromServices] UserManager userManager)
	{
		var user = await userManager.GetUserAsync(httpContext.User);
		if (user is null)
		{
			return TypedResults.InternalServerError();
		}

		return TypedResults.Ok(new UserData
		{
			Id = user.Id,
			DisplayName = user.DisplayName,
			Description = user.Description,
			CurrentRoomId = user.CurrentRoomId,
			Type = user.Type,
			Presence = user.Presence,
			Permissions = user.Permissions,
		});
	}
}
