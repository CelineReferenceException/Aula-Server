using FluentValidation;

namespace Aula.Server.Domain.Bans;

internal sealed class BanValidator : AbstractValidator<Ban>
{
	public BanValidator()
	{
		var banTypes = Enum.GetValues<BanType>();

		_ = RuleFor(x => x.Id)
			.NotNull()
			.WithErrorCode(nameof(Ban.Id).ToCamelCase())
			.WithMessage("Required.");

		_ = RuleFor(x => x.Type)
			.IsInEnum()
			.WithErrorCode(nameof(Ban.Type).ToCamelCase())
			.WithMessage($"Unknown type. Known values are {String.Join(", ", banTypes.Cast<Int32>())}.");

		_ = RuleFor(x => x.Reason)
			.MinimumLength(Ban.ReasonMinimumLength)
			.WithErrorCode(nameof(Ban.Reason).ToCamelCase())
			.WithMessage($"Length must be at least {Ban.ReasonMinimumLength}.");

		_ = RuleFor(x => x.Reason)
			.MaximumLength(Ban.ReasonMaximumLength)
			.WithErrorCode(nameof(Ban.Reason).ToCamelCase())
			.WithMessage($"Length must be at most {Ban.ReasonMaximumLength}.");

		_ = When(x => x.Type is BanType.Id, () =>
		{
			_ = RuleFor(x => x.TargetId)
				.NotNull()
				.WithErrorCode(nameof(Ban.TargetId).ToCamelCase())
				.WithMessage($"Required when {nameof(Type).ToCamelCase()} is {(Int32)BanType.Id}.");
		});
	}

	internal static BanValidator Instance { get; } = new();
}
