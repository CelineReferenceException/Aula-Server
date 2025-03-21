using FluentValidation;

namespace Aula.Server.Core.Features.Bans;

internal sealed class CreateBanRequestBodyValidator : AbstractValidator<CreateBanRequestBody>
{
	public CreateBanRequestBodyValidator()
	{
		_ = RuleFor(x => x.Reason)
			.MinimumLength(Ban.ReasonMinimumLength)
			.WithErrorCode($"{nameof(CreateBanRequestBody.Reason)} is too short")
			.WithMessage($"{nameof(CreateBanRequestBody.Reason)} length must be at least {Ban.ReasonMinimumLength}");

		_ = RuleFor(x => x.Reason)
			.MaximumLength(Ban.ReasonMaximumLength)
			.WithErrorCode($"{nameof(CreateBanRequestBody.Reason)} is too long")
			.WithMessage($"{nameof(CreateBanRequestBody.Reason)} length must be at most {Ban.ReasonMaximumLength}");
	}
}
