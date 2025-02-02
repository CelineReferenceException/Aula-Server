using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WhiteTale.Server.Features.Messages.Endpoints;

internal static class ProblemDetailsDefaults
{
	internal static ProblemDetails RoomDoesNotExist { get; } = new()
	{
		Title = "Invalid room",
		Detail = "The specified room does not exist",
		Status = StatusCodes.Status400BadRequest,
	};

	internal static ProblemDetails UserIsNotInTheRoom { get; } = new()
	{
		Title = "Invalid room",
		Detail = "The current user is not in the room",
		Status = StatusCodes.Status403Forbidden,
	};

	internal static ProblemDetails InvalidMessageCount { get; } = new()
	{
		Title = "Invalid message count.",
		Detail = $"The message count must be between {GetMessages.MinimumMessageCount} and {GetMessages.MaximumMessageCount}.",
		Status = StatusCodes.Status400BadRequest,
	};

	internal static ProblemDetails InvalidBeforeMessage { get; } = new()
	{
		Title = $"Invalid '{GetMessages.BeforeQueryParameter}' query parameter.",
		Detail = "A message with the specified ID was not found.",
		Status = StatusCodes.Status400BadRequest,
	};

	internal static ProblemDetails InvalidAfterMessage { get; } = new()
	{
		Title = $"Invalid '{GetMessages.AfterQueryParameter}' query parameter.",
		Detail = "A message with the specified ID was not found.",
		Status = StatusCodes.Status400BadRequest,
	};
}
