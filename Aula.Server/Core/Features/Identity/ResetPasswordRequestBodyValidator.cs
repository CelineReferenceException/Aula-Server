using Aula.Server.Core.Identity;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace Aula.Server.Core.Features.Identity;

internal sealed class ResetPasswordRequestBodyValidator : AbstractValidator<ResetPasswordRequestBody>
{
	public ResetPasswordRequestBodyValidator(IOptions<IdentityOptions> options)
	{
		_ = RuleFor(x => x.Code)
			.NotEmpty()
			.WithErrorCode(nameof(ResetPasswordRequestBody.Code))
			.WithMessage("Cannot be null or empty");

		_ = RuleFor(x => x.NewPassword)
			.MinimumLength(options.Value.Password.RequiredLength)
			.WithErrorCode(nameof(ResetPasswordRequestBody.NewPassword).ToCamel())
			.WithMessage($"Length must be at least {options.Value.Password.RequiredLength}");

		_ = RuleFor(x => x.NewPassword)
			.MaximumLength(User.PasswordMaximumLength)
			.WithErrorCode(nameof(ResetPasswordRequestBody.NewPassword).ToCamel())
			.WithMessage($"Length must be at most {User.PasswordMaximumLength}");
	}
}
