using FluentValidation;

namespace Aula.Server.Core.Features.Identity;

internal sealed class RegisterRequestBodyValidator : AbstractValidator<RegisterRequestBody>
{
	public RegisterRequestBodyValidator()
	{
		_ = RuleFor(x => x.DisplayName)
			.MinimumLength(User.DisplayNameMinimumLength)
			.WithErrorCode(nameof(RegisterRequestBody.DisplayName).ToCamel())
			.WithMessage($"Length must be at least {User.DisplayNameMinimumLength}");

		_ = RuleFor(x => x.DisplayName)
			.MaximumLength(User.DisplayNameMaximumLength)
			.WithErrorCode(nameof(RegisterRequestBody.DisplayName).ToCamel())
			.WithMessage($"Length must be at most {User.DisplayNameMaximumLength}");

		_ = RuleFor(x => x.UserName)
			.MinimumLength(User.UserNameMinimumLength)
			.WithErrorCode(nameof(RegisterRequestBody.UserName).ToCamel())
			.WithMessage($"Length must be at least {User.DisplayNameMinimumLength}");

		_ = RuleFor(x => x.UserName)
			.MaximumLength(User.UserNameMaximumLength)
			.WithErrorCode(nameof(RegisterRequestBody.UserName).ToCamel())
			.WithMessage($"Length must be at most {User.DisplayNameMaximumLength}");

		_ = RuleFor(x => x.Email)
			.EmailAddress()
			.WithErrorCode(nameof(RegisterRequestBody.Email).ToCamel())
			.WithMessage($"Must be a valid email address");

		_ = RuleFor(x => x.Password)
			.NotEmpty()
			.WithErrorCode($"{nameof(RegisterRequestBody.Password)} is empty")
			.WithMessage($"{nameof(RegisterRequestBody.Password)} cannot be empty.");
	}
}
