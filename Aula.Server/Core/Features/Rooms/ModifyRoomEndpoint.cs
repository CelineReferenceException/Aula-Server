using Aula.Server.Core.Authorization;
using Aula.Server.Core.Domain;
using Aula.Server.Core.Domain.Users;
using Aula.Server.Core.Endpoints;
using Aula.Server.Core.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Core.Features.Rooms;

internal sealed class ModifyRoomEndpoint : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapPatch("rooms/{roomId}", HandleAsync)
			.RequireAuthenticatedUser()
			.RequirePermissions(Permissions.ManageRooms)
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<Ok<RoomData>, NotFound, ProblemHttpResult, InternalServerError>> HandleAsync(
		[FromRoute] Snowflake roomId,
		[FromBody] ModifyRoomRequestBody body,
		[FromServices] ModifyRoomRequestBodyValidator bodyValidator,
		[FromServices] ApplicationDbContext dbContext)
	{
		var validation = await bodyValidator.ValidateAsync(body);
		if (!validation.IsValid)
		{
			var problemDetails = validation.Errors.ToProblemDetails();
			return TypedResults.Problem(problemDetails);
		}

		var room = await dbContext.Rooms
			.AsTracking()
			.Where(r => r.Id == roomId && !r.IsRemoved)
			.Include(r => r.Connections)
			.FirstOrDefaultAsync();
		if (room is null)
		{
			return TypedResults.NotFound();
		}

		room.Modify(body.Name, body.Description, body.IsEntrance).ThrowIfFailed();
		room.UpdateConcurrencyStamp();

		try
		{
			_ = await dbContext.SaveChangesAsync();
		}
		catch (DbUpdateConcurrencyException)
		{
			return TypedResults.InternalServerError();
		}

		return TypedResults.Ok(new RoomData
		{
			Id = room.Id,
			Name = room.Name,
			Description = room.Description,
			IsEntrance = room.IsEntrance,
			ConnectedRoomIds = room.Connections.Select(c => c.TargetRoomId).ToArray(),
			CreationDate = room.CreationDate,
		});
	}
}
