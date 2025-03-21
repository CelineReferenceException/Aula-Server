﻿using Aula.Server.Core.Authorization;
using Aula.Server.Core.Endpoints;
using Aula.Server.Core.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Aula.Server.Core.Features.Rooms;

internal sealed class CreateRoomEndpoint : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapPost("rooms", HandleAsync)
			.RequireAuthenticatedUser()
			.RequirePermissions(Permissions.ManageRooms)
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<Created<RoomData>, ProblemHttpResult>> HandleAsync(
		[FromBody] CreateRoomRequestBody body,
		[FromServices] CreateRoomRequestBodyValidator bodyValidator,
		[FromServices] ApplicationDbContext dbContext,
		[FromServices] SnowflakeGenerator snowflakeGenerator,
		HttpContext httpContext)
	{
		var validation = await bodyValidator.ValidateAsync(body);
		if (!validation.IsValid)
		{
			var problemDetails = validation.Errors.ToProblemDetails();
			return TypedResults.Problem(problemDetails);
		}

		var room = new Room(await snowflakeGenerator.NewSnowflakeAsync(), body.Name, body.Description, body.IsEntrance ?? false);

		_ = dbContext.Rooms.Add(room);
		_ = await dbContext.SaveChangesAsync();

		return TypedResults.Created($"{httpContext.Request.GetUrl()}/{room.Id}", new RoomData
		{
			Id = room.Id,
			Name = room.Name,
			Description = room.Description,
			IsEntrance = room.IsEntrance,
			CreationDate = room.CreationDate,
			ConnectedRoomIds = [],
		});
	}
}
