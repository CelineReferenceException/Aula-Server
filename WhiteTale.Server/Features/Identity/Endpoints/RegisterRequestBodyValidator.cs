using FluentValidation;

namespace WhiteTale.Server.Features.Identity.Endpoints;

internal sealed class RegisterRequestBodyValidator : AbstractValidator<RegisterRequestBody>
{
	public RegisterRequestBodyValidator()
	{
		_ = RuleFor(x => x.DisplayName)
			.MinimumLength(User.DisplayNameMinimumLength)
			.WithErrorCode($"{nameof(RegisterRequestBody.DisplayName)} is too short")
			.WithMessage($"{nameof(RegisterRequestBody.DisplayName)} length must be at least {User.DisplayNameMinimumLength}");

		_ = RuleFor(x => x.DisplayName)
			.MaximumLength(User.DisplayNameMaximumLength)
			.WithErrorCode($"{nameof(RegisterRequestBody.DisplayName)} is too long")
			.WithMessage($"{nameof(RegisterRequestBody.DisplayName)} length must be at most {User.DisplayNameMaximumLength}");

		_ = RuleFor(x => x.UserName)
			.NotEmpty()
			.WithErrorCode($"{nameof(RegisterRequestBody.UserName)} is empty")
			.WithMessage($"{nameof(RegisterRequestBody.UserName)} cannot be empty.");

		_ = RuleFor(x => x.UserName)
			.MinimumLength(User.UserNameMinimumLength)
			.WithErrorCode($"{nameof(RegisterRequestBody.UserName)} is too short")
			.WithMessage($"{nameof(RegisterRequestBody.UserName)} length must be at least {User.DisplayNameMinimumLength}");

		_ = RuleFor(x => x.UserName)
			.MaximumLength(User.UserNameMaximumLength)
			.WithErrorCode($"{nameof(RegisterRequestBody.UserName)} is too long")
			.WithMessage($"{nameof(RegisterRequestBody.UserName)} length must be at most {User.DisplayNameMaximumLength}");

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
