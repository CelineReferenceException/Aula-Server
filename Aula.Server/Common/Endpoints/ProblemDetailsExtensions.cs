using FluentValidation.Results;
using Microsoft.AspNetCore.Http;

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
}
