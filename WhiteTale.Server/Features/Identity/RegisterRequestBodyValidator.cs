using WhiteTale.Server.Domain.Characters;

namespace WhiteTale.Server.Features.Identity;

internal sealed class RegisterRequestBodyValidator : AbstractValidator<RegisterRequestBody>
{
	public RegisterRequestBodyValidator()
	{
		_ = RuleFor(x => x.DisplayName)
			.MinimumLength(Character.DisplayNameMinimumLength)
			.WithErrorCode($"{nameof(RegisterRequestBody.DisplayName)} is too short")
			.WithMessage($"{nameof(RegisterRequestBody.DisplayName)} length must be at least {Character.DisplayNameMinimumLength}");

		_ = RuleFor(x => x.DisplayName)
			.MaximumLength(Character.DisplayNameMaximumLength)
			.WithErrorCode($"{nameof(RegisterRequestBody.DisplayName)} is too long")
			.WithMessage($"{nameof(RegisterRequestBody.DisplayName)} length must be at most {Character.DisplayNameMaximumLength}");

		_ = RuleFor(x => x.UserName)
			.NotEmpty()
			.WithErrorCode($"{nameof(RegisterRequestBody.UserName)} is empty")
			.WithMessage($"{nameof(RegisterRequestBody.UserName)} cannot be empty.");

		_ = RuleFor(x => x.UserName)
			.MinimumLength(Character.UserNameMinimumLength)
			.WithErrorCode($"{nameof(RegisterRequestBody.UserName)} is too short")
			.WithMessage($"{nameof(RegisterRequestBody.UserName)} length must be at least {Character.DisplayNameMinimumLength}");

		_ = RuleFor(x => x.UserName)
			.MaximumLength(Character.UserNameMaximumLength)
			.WithErrorCode($"{nameof(RegisterRequestBody.UserName)} is too long")
			.WithMessage($"{nameof(RegisterRequestBody.UserName)} length must be at most {Character.DisplayNameMaximumLength}");

		_ = RuleFor(x => x.Email)
			.NotEmpty()
			.WithErrorCode($"{nameof(RegisterRequestBody.Email)} is empty")
			.WithMessage($"{nameof(RegisterRequestBody.Email)} cannot be empty.");

		_ = RuleFor(x => x.Email)
			.EmailAddress()
			.WithErrorCode($"Invalid {nameof(RegisterRequestBody.Email)}")
			.WithMessage($"{nameof(RegisterRequestBody.Email)} is not a valid email address.");

		_ = RuleFor(x => x.Password)
			.NotEmpty()
			.WithErrorCode($"{nameof(RegisterRequestBody.Password)} is empty")
			.WithMessage($"{nameof(RegisterRequestBody.Password)} cannot be empty.");
	}
}
