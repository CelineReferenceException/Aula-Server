namespace Aula.Server.Common.Results;

internal sealed class ResultProblem
{
	internal ResultProblem(String name, String description)
	{
		Name = name;
		Description = description;
	}

	internal String Name { get; }

	internal String Description { get; }
}
