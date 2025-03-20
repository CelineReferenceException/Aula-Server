namespace Aula.Server.Common.Validation;

internal sealed class CustomValidationProblem
{
	internal CustomValidationProblem(String title, String details)
	{
		Title = title;
		Details = details;
	}

	internal String Title { get; }

	internal String Details { get; }
}
