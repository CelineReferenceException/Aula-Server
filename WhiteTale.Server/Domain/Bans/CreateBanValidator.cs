using FluentValidation;

namespace WhiteTale.Server.Domain.Bans;

internal sealed class CreateBanValidator : AbstractValidator<Ban>
{
	public CreateBanValidator()
	{
		_ = RuleFor(x => x.Id).NotEmpty();

		_ = RuleFor(x => x.Type)
			.IsInEnum();

		_ = RuleFor(x => x.Reason)
			.MinimumLength(Ban.ReasonMinimumLength)
			.MaximumLength(Ban.ReasonMaximumLength);

		_ = When(x => x.Type is BanType.Id, () =>
		{
			_ = RuleFor(x => x.TargetId).NotEmpty();
		});
	}
}
