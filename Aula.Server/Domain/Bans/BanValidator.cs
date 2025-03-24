using FluentValidation;

namespace Aula.Server.Domain.Bans;

internal sealed class BanValidator : AbstractValidator<Ban>
{
	public BanValidator()
	{
		_ = RuleFor(x => x.Id).NotNull();

		_ = RuleFor(x => x.Type)
			.IsInEnum();

		_ = RuleFor(x => x.Reason)
			.MinimumLength(Ban.ReasonMinimumLength)
			.MaximumLength(Ban.ReasonMaximumLength);

		_ = When(x => x.Type is BanType.Id, () => { _ = RuleFor(x => x.TargetId).NotNull(); });
	}
}
