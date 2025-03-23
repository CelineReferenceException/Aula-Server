using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Aula.Server.Core.Endpoints;

internal static class ProblemDetailsExtensions
{
	[Obsolete]
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

	internal static ValidationProblemDetails ToProblemDetails(this Result<Object> result)
	{
		var propertyProblems = new Dictionary<String, List<String>>();

		foreach (var problem in result.Problems)
		{
			if (propertyProblems.TryGetValue(problem.Name, out var problems))
			{
				problems.Add(problem.Description);
			}
			else
			{
				propertyProblems.Add(problem.Name, [problem.Description,]);
			}
		}

		return new ValidationProblemDetails
		{
			Status = StatusCodes.Status400BadRequest,
			Title = "Validation problem",
			Detail = "One or more validation errors occurred.",
			Errors = propertyProblems.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToArray(), StringComparer.OrdinalIgnoreCase),
		};
	}
}
