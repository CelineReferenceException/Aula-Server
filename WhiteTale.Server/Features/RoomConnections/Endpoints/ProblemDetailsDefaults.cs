using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WhiteTale.Server.Features.RoomConnections.Endpoints;

internal static class ProblemDetailsDefaults
{
	internal static ProblemDetails TargetRoomCannotBeSourceRoom { get; } = new()
	{
		Title = "Invalid target room",
		Detail = "The specified target room cannot be the same as the source room.",
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
		Detail = "The specified target room does not exist.",
		Status = StatusCodes.Status400BadRequest,
	};
}
