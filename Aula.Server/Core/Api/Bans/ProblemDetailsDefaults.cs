﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Aula.Server.Core.Api.Bans;

internal static class ProblemDetailsDefaults
{
	internal static ProblemDetails UserAlreadyBanned { get; } = new()
	{
		Title = "Invalid operation",
		Detail = "The specified user is already banned.",
		Status = StatusCodes.Status409Conflict,
	};

	internal static ProblemDetails TargetDoesNotExist { get; } = new()
	{
		Title = "Invalid target",
		Detail = "The specified user does not exist.",
		Status = StatusCodes.Status400BadRequest,
	};

	internal static ProblemDetails TargetIsAdministrator { get; } = new()
	{
		Title = "Invalid operation",
		Detail = "The specified user has administrator permissions.",
		Status = StatusCodes.Status403Forbidden,
	};

	internal static ProblemDetails InvalidBanCount { get; } = new()
	{
		Title = "Invalid ban count.",
		Detail = $"The ban count must be between {GetBansEndpoint.MinimumBanCount} and {GetBansEndpoint.MaximumBanCount}.",
		Status = StatusCodes.Status400BadRequest,
	};

	internal static ProblemDetails InvalidAfterUser { get; } = new()
	{
		Title = $"Invalid '{GetBansEndpoint.AfterQueryParameter}' query parameter.",
		Detail = "A ban with the specified user ID was not found.",
		Status = StatusCodes.Status400BadRequest,
	};
}
