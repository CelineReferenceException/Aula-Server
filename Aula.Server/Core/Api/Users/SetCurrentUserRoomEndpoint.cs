using Aula.Server.Common.Authorization;
using Aula.Server.Common.Endpoints;
using Aula.Server.Common.Identity;
using Aula.Server.Common.Persistence;
using Aula.Server.Domain.Users;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Core.Api.Users;

internal sealed class SetCurrentUserRoomEndpoint : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapPut("users/@me/current-room/", HandleAsync)
			.RequireAuthenticatedUser()
			.RequirePermissions(Permissions.SetOwnCurrentRoom)
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<NoContent, ProblemHttpResult, InternalServerError>> HandleAsync(
		[FromBody] SetUserRoomRequestBody body,
		[FromServices] ApplicationDbContext dbContext,
		[FromServices] UserManager userManager,
		HttpContext httpContext)
	{
		var user = await userManager.GetUserAsync(httpContext.User);
		if (user is null)
		{
			return TypedResults.InternalServerError();
		}

		if (user.CurrentRoomId == body.RoomId)
		{
			return TypedResults.NoContent();
		}

		var room = await dbContext.Rooms
			.Where(r => r.Id == body.RoomId && !r.IsRemoved)
			.Select(r => new
			{
				r.IsEntrance,
			})
			.FirstOrDefaultAsync();
		if (room is null)
		{
			return TypedResults.Problem(ProblemDetailsDefaults.RoomDoesNotExist);
		}

		if (user.CurrentRoomId is null &&
		    !room.IsEntrance)
		{
			return TypedResults.Problem(ProblemDetailsDefaults.RoomIsNotEntrance);
		}

		if (user.CurrentRoomId is not null &&
		    !await dbContext.RoomConnections.AnyAsync(r => r.SourceRoomId == user.CurrentRoomId && r.TargetRoomId == body.RoomId))
		{
			return TypedResults.Problem(ProblemDetailsDefaults.NoRoomConnection);
		}

		// We fetch the user entity from the DbContext because we don't want to modify the one cached by the UserManager.
		var u = await dbContext.Users
			.AsTracking()
			.Where(x => x.Id == user.Id)
			.FirstOrDefaultAsync();
		if (u is null)
		{
			return TypedResults.InternalServerError();
		}

		u.SetCurrentRoom(body.RoomId);
		u.UpdateConcurrencyStamp();

		try
		{
			_ = await dbContext.SaveChangesAsync();
		}
		catch (DbUpdateConcurrencyException)
		{
			return TypedResults.InternalServerError();
		}

		return TypedResults.NoContent();
	}
}
