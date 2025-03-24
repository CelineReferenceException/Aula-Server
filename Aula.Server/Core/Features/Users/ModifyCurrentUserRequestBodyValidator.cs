using FluentValidation;

namespace Aula.Server.Core.Features.Users;

internal sealed class ModifyCurrentUserRequestBodyValidator : AbstractValidator<ModifyCurrentUserRequestBody>
{
	public ModifyCurrentUserRequestBodyValidator()
	{
		_ = RuleFor(x => x.DisplayName)
			.MinimumLength(User.DisplayNameMinimumLength)
			.WithErrorCode(nameof(ModifyCurrentUserRequestBody.DisplayName).ToCamel())
			.WithMessage($"Length must be at least {User.DisplayNameMinimumLength}");

		_ = RuleFor(x => x.DisplayName)
			.MaximumLength(User.DisplayNameMaximumLength)
			.WithErrorCode(nameof(ModifyCurrentUserRequestBody.DisplayName).ToCamel())
			.WithMessage($"Length must be at most {User.DisplayNameMaximumLength}");

		_ = RuleFor(x => x.Description)
			.MaximumLength(User.DescriptionMaximumLength)
			.WithErrorCode(nameof(ModifyCurrentUserRequestBody.Description).ToCamel())
			.WithMessage($"Length must be at most {User.DescriptionMaximumLength}");
	}
}
