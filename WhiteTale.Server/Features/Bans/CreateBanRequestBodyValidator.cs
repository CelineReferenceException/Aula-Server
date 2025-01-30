using FluentValidation;

namespace WhiteTale.Server.Features.Bans;

internal sealed class CreateBanRequestBodyValidator : AbstractValidator<CreateBanRequestBody>
{
	public CreateBanRequestBodyValidator()
	{
		_ = RuleFor(x => x.Reason)
			.MinimumLength(Ban.ReasonMinimumLength)
			.WithErrorCode($"{nameof(Ban.Reason)} is too short")
			.WithMessage($"{nameof(Ban.Reason)} length must be at least {User.DisplayNameMinimumLength}");

		_ = RuleFor(x => x.Reason)
			.MaximumLength(Ban.ReasonMaximumLength)
			.WithErrorCode($"{nameof(Ban.Reason)} is too long")
			.WithMessage($"{nameof(Ban.ReasonMaximumLength)} length must be at most {User.DisplayNameMaximumLength}");
	}
}
