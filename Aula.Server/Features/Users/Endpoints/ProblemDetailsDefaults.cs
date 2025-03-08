using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Aula.Server.Features.Users.Endpoints;

internal static class ProblemDetailsDefaults
{
	internal static ProblemDetails UserDoesNotExist { get; } = new()
	{
		Title = "Invalid user",
		Detail = "The specified user does not exist.",
		Status = StatusCodes.Status400BadRequest,
	};

	internal static ProblemDetails RoomDoesNotExist { get; } = new()
	{
		Title = "Invalid room",
		Detail = "The specified room does not exist.",
		Status = StatusCodes.Status400BadRequest,
	};

	internal static ProblemDetails RoomIsNotEntrance { get; } = new()
	{
		Title = "Invalid room",
		Detail = "The specified room is not an entrance.",
		Status = StatusCodes.Status400BadRequest,
	};

	internal static ProblemDetails NoRoomConnection { get; } = new()
	{
		Title = "Room connection required",
		Detail = "The current room is not connected to the specified room.",
		Status = StatusCodes.Status400BadRequest,
	};

	internal static ProblemDetails InvalidUserCount { get; } = new()
	{
		Title = "Invalid user count.",
		Detail = $"The message count must be between {GetUsers.MinimumUserCount} and {GetUsers.MaximumUserCount}.",
		Status = StatusCodes.Status400BadRequest,
	};

	internal static ProblemDetails InvalidAfterUser { get; } = new()
	{
		Title = $"Invalid '{GetUsers.AfterQueryParameter}' query parameter.",
		Detail = "A message with the specified ID was not found.",
		Status = StatusCodes.Status400BadRequest,
	};
}
