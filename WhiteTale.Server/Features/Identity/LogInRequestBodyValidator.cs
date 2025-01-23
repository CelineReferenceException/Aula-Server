using FluentValidation;

namespace WhiteTale.Server.Features.Identity;

internal sealed class LogInRequestBodyValidator : AbstractValidator<LogInRequestBody>
{
	public LogInRequestBodyValidator()
	{
		_ = RuleFor(x => x.UserName)
			.NotEmpty()
			.WithErrorCode($"{nameof(LogInRequestBody.UserName)} is empty")
			.WithMessage($"{nameof(LogInRequestBody.UserName)} cannot be empty.");

		_ = RuleFor(x => x.Password)
			.NotEmpty()
			.WithErrorCode($"{nameof(LogInRequestBody.Password)} is empty")
			.WithMessage($"{nameof(LogInRequestBody.Password)} cannot be empty.");
	}
}
