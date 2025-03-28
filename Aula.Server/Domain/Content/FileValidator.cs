using FluentValidation;

namespace Aula.Server.Domain.Content;

internal sealed class FileValidator : AbstractValidator<File>
{
	public FileValidator()
	{
		_ = RuleFor(x => x.Name).MinimumLength(File.NameMinimumLength);
		_ = RuleFor(x => x.Name).MaximumLength(File.NameMaximumLength);
		_ = RuleFor(x => x.Extension).MinimumLength(File.ExtensionMinimumLength);
		_ = RuleFor(x => x.Extension).MaximumLength(File.ExtensionMaximumLength);
	}

	internal static FileValidator Instance { get; } = new();
}
