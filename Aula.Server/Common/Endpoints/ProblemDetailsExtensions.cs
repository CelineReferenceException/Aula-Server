using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Aula.Server.Common.Endpoints;

internal static class ProblemDetailsExtensions
{
	internal static HttpValidationProblemDetails ToProblemDetails(this IEnumerable<ValidationFailure> validationFailures)
	{
		var problemErrors = validationFailures
			.Select(static failure => new KeyValuePair<String, String[]>(failure.ErrorCode, [failure.ErrorMessage,]))
			.ToDictionary();

		return new HttpValidationProblemDetails
		{
			Status = StatusCodes.Status400BadRequest,
			Title = "Validation problem",
			Detail = "One or more validation errors occurred.",
			Errors = problemErrors,
		};
	}

	internal static HttpValidationProblemDetails ToProblemDetails(this IEnumerable<IdentityError> identityErrors)
	{
		var problemErrors = identityErrors
			.Select(static error => new KeyValuePair<String, String[]>(error.Code, [error.Description,]))
			.ToDictionary();

		return new HttpValidationProblemDetails
		{
			Status = StatusCodes.Status400BadRequest,
			Title = "Identity problem",
			Detail = "One or more identity errors occurred.",
			Errors = problemErrors,
		};
	}
}
