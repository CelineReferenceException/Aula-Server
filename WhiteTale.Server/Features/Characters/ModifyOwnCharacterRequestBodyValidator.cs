using WhiteTale.Server.Domain.Characters;

namespace WhiteTale.Server.Features.Characters;

internal sealed class ModifyOwnCharacterRequestBodyValidator : AbstractValidator<ModifyOwnCharacterRequestBody>
{
	public ModifyOwnCharacterRequestBodyValidator()
	{
		_ = RuleFor(x => x.DisplayName)
			.MinimumLength(Character.DisplayNameMinimumLength)
			.WithErrorCode($"{nameof(ModifyOwnCharacterRequestBody.DisplayName)} is too short")
			.WithMessage(
				$"{nameof(ModifyOwnCharacterRequestBody.DisplayName)} length must be at least {Character.DisplayNameMinimumLength}");

		_ = RuleFor(x => x.DisplayName)
			.MaximumLength(Character.DisplayNameMaximumLength)
			.WithErrorCode($"{nameof(ModifyOwnCharacterRequestBody.DisplayName)} is too long")
			.WithMessage(
				$"{nameof(ModifyOwnCharacterRequestBody.DisplayName)} length must be at most {Character.DisplayNameMaximumLength}");

		_ = RuleFor(x => x.Description)
			.MinimumLength(Character.DescriptionMinimumLength)
			.WithErrorCode($"{nameof(ModifyOwnCharacterRequestBody.Description)} is too long")
			.WithMessage(
				$"{nameof(ModifyOwnCharacterRequestBody.Description)} length must be at most {Character.DisplayNameMaximumLength}");

		_ = RuleFor(x => x.Description)
			.MaximumLength(Character.DescriptionMaximumLength)
			.WithErrorCode($"{nameof(ModifyOwnCharacterRequestBody.Description)} is too long")
			.WithMessage(
				$"{nameof(ModifyOwnCharacterRequestBody.Description)} length must be at most {Character.DisplayNameMaximumLength}");
	}
}
