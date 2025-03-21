using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Aula.Server.Core.Features.Rooms.Endpoints;

internal static class ProblemDetailsDefaults
{
	internal static ProblemDetails InvalidRoomCount { get; } = new()
	{
		Title = "Invalid message count.",
		Detail = $"The message count must be between {GetRooms.MinimumRoomCount} and {GetRooms.MaximumRoomCount}.",
		Status = StatusCodes.Status400BadRequest,
	};

	internal static ProblemDetails InvalidAfterRoom { get; } = new()
	{
		Title = $"Invalid '{GetRooms.AfterQueryParameter}' query parameter.",
		Detail = "A message with the specified ID was not found.",
		Status = StatusCodes.Status400BadRequest,
	};

	internal static ProblemDetails TargetRoomCannotBeSourceRoom { get; } = new()
	{
		Title = "Invalid target room",
		Detail = "A target room cannot be the same as the source room.",
		Status = StatusCodes.Status400BadRequest,
	};

	internal static ProblemDetails RoomDoesNotExist { get; } = new()
	{
		Title = "Invalid room",
		Detail = "The specified room does not exist.",
		Status = StatusCodes.Status400BadRequest,
	};

	internal static ProblemDetails TargetRoomDoesNotExist { get; } = new()
	{
		Title = "Invalid target room",
		Detail = "A specified target room does not exist.",
		Status = StatusCodes.Status400BadRequest,
	};
}
