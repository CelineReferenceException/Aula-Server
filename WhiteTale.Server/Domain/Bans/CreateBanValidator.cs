using FluentValidation;

namespace WhiteTale.Server.Domain.Bans;

internal sealed class CreateBanValidator : AbstractValidator<Ban>
{
	internal CreateBanValidator()
	{
		_ = RuleFor(x => x.Id).NotEmpty();

		_ = RuleFor(x => x.Type)
			.NotEmpty()
			.IsInEnum();

		_ = RuleFor(x => x.Reason)
			.MinimumLength(Ban.ReasonMinimumLength)
			.MaximumLength(Ban.ReasonMaximumLength);

		_ = RuleFor(x => x.TargetId).NotEmpty().When(x => x.Type is BanType.Id);

		_ = RuleFor(x => x.IpAddress).NotEmpty().When(x => x.Type is BanType.IpAddress);
	}
}
