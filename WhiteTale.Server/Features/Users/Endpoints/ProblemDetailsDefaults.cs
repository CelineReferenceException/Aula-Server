using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WhiteTale.Server.Features.Users.Endpoints;

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
}
