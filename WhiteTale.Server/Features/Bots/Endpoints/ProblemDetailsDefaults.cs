using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WhiteTale.Server.Features.Bots.Endpoints;

internal static class ProblemDetailsDefaults
{
	internal static ProblemDetails UserDoesNotExist { get; } = new()
	{
		Title = "Invalid user",
		Detail = "The specified user does not exist.",
		Status = StatusCodes.Status400BadRequest,
	};
}
