namespace Aula.Server.Common.Results;

internal sealed class ResultProblem
{
	internal ResultProblem(String title, String details)
	{
		Title = title;
		Details = details;
	}

	internal String Title { get; }

	internal String Details { get; }
}
