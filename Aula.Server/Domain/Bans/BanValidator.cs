using FluentValidation;

namespace Aula.Server.Domain.Bans;

internal sealed class BanValidator : AbstractValidator<Ban>
{
	public BanValidator()
	{
		_ = RuleFor(x => x.Type).IsInEnum();
		_ = RuleFor(x => x.Reason).MinimumLength(Ban.ReasonMinimumLength);
		_ = RuleFor(x => x.Reason).MaximumLength(Ban.ReasonMaximumLength);
		_ = When(x => x.Type is BanType.Id, () =>
		{
			_ = RuleFor(x => x.TargetId).NotNull();
		});
	}

	internal static BanValidator Instance { get; } = new();
}
