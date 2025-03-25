using Aula.Server.Core.Domain.Bans;
using FluentValidation;

namespace Aula.Server.Core.Features.Bans;

internal sealed class CreateBanRequestBodyValidator : AbstractValidator<CreateBanRequestBody>
{
	public CreateBanRequestBodyValidator()
	{
		_ = RuleFor(x => x.Reason)
			.MinimumLength(Ban.ReasonMinimumLength)
			.WithErrorCode(nameof(CreateBanRequestBody.Reason).ToCamel())
			.WithMessage($"Length must be at least {Ban.ReasonMinimumLength}");

		_ = RuleFor(x => x.Reason)
			.MaximumLength(Ban.ReasonMaximumLength)
			.WithErrorCode(nameof(CreateBanRequestBody.Reason).ToCamel())
			.WithMessage($"Length must be at most {Ban.ReasonMaximumLength}");
	}
}
