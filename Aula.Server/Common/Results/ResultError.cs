namespace Aula.Server.Common.Results;

[Obsolete]
internal sealed class ResultError
{
	internal ResultError(String name, String description)
	{
		Name = name;
		Description = description;
	}

	internal String Name { get; }

	internal String Description { get; }
}
