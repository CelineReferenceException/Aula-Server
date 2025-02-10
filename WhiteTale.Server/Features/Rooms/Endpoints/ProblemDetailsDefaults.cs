using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WhiteTale.Server.Features.Rooms.Endpoints;

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
}
