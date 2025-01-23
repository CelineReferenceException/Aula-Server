using FluentValidation;

namespace WhiteTale.Server.Features.Identity;

internal sealed class ResetPasswordRequestBodyValidator : AbstractValidator<ResetPasswordRequestBody>
{
	public ResetPasswordRequestBodyValidator()
	{
		_ = RuleFor(x => x.UserId)
			.NotNull()
			.WithErrorCode($"{nameof(ResetPasswordRequestBody.UserId)} is null")
			.WithMessage($"{nameof(ResetPasswordRequestBody.UserId)} cannot be null.");

		_ = RuleFor(x => x.ResetToken)
			.NotEmpty()
			.WithErrorCode($"{nameof(ResetPasswordRequestBody.ResetToken)} is empty")
			.WithMessage($"{nameof(ResetPasswordRequestBody.ResetToken)} cannot be empty.");

		_ = RuleFor(x => x.NewPassword)
			.NotEmpty()
			.WithErrorCode($"{nameof(ResetPasswordRequestBody.NewPassword)} is empty")
			.WithMessage($"{nameof(ResetPasswordRequestBody.NewPassword)} cannot be empty.");
	}
}
