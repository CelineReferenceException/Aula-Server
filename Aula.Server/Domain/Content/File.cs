using FluentValidation.Results;

namespace Aula.Server.Domain.Content;

internal sealed class File
{
	internal const Int32 NameMinimumLength = 1;
	internal const Int32 NameMaximumLength = 128;
	internal const Int32 ExtensionMinimumLength = 1;
	internal const Int32 ExtensionMaximumLength = 8;

	private File(Snowflake id, String name, String extension)
	{
		Id = id;
		Name = name;
		Extension = extension;
	}

	internal Snowflake Id { get; }

	internal String Name { get; }

	internal String Extension { get; }

	internal static Result<File, ValidationFailure> Create(Snowflake id, String name, String extension)
	{
		var file = new File(id, name, extension);

		var validationResult = FileValidator.Instance.Validate(file);
		return validationResult.IsValid
			? file
			: new ResultErrorValues<ValidationFailure>(validationResult.Errors);
	}
}
