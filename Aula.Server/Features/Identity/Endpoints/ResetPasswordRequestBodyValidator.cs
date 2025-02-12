using FluentValidation;

namespace Aula.Server.Features.Identity.Endpoints;

internal sealed class ResetPasswordRequestBodyValidator : AbstractValidator<ResetPasswordRequestBody>
{
	public ResetPasswordRequestBodyValidator()
	{
		_ = RuleFor(x => x.Code)
			.NotEmpty()
			.WithErrorCode($"{nameof(ResetPasswordRequestBody.Code)} is empty")
			.WithMessage($"{nameof(ResetPasswordRequestBody.Code)} cannot be empty.");

		_ = RuleFor(x => x.NewPassword)
			.NotEmpty()
			.WithErrorCode($"{nameof(ResetPasswordRequestBody.NewPassword)} is empty")
			.WithMessage($"{nameof(ResetPasswordRequestBody.NewPassword)} cannot be empty.");
	}
}
