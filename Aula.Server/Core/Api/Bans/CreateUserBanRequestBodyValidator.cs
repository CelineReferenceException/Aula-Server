using Aula.Server.Domain.Bans;
using FluentValidation;

namespace Aula.Server.Core.Api.Bans;

internal sealed class CreateUserBanRequestBodyValidator : AbstractValidator<CreateUserBanRequestBody>
{
	public CreateUserBanRequestBodyValidator()
	{
		_ = RuleFor(x => x.Reason)
			.MinimumLength(Ban.ReasonMinimumLength)
			.WithErrorCode(nameof(CreateUserBanRequestBody.Reason).ToCamel())
			.WithMessage($"Length must be at least {Ban.ReasonMinimumLength}");

		_ = RuleFor(x => x.Reason)
			.MaximumLength(Ban.ReasonMaximumLength)
			.WithErrorCode(nameof(CreateUserBanRequestBody.Reason).ToCamel())
			.WithMessage($"Length must be at most {Ban.ReasonMaximumLength}");
	}
}
