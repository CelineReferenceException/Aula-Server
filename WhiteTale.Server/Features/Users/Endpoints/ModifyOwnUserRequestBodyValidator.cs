using FluentValidation;

namespace WhiteTale.Server.Features.Users.Endpoints;

internal sealed class ModifyOwnUserRequestBodyValidator : AbstractValidator<ModifyOwnUserRequestBody>
{
	public ModifyOwnUserRequestBodyValidator()
	{
		_ = RuleFor(x => x.DisplayName)
			.MinimumLength(User.DisplayNameMinimumLength)
			.WithErrorCode($"{nameof(ModifyOwnUserRequestBody.DisplayName)} is too short")
			.WithMessage($"{nameof(ModifyOwnUserRequestBody.DisplayName)} length must be at least {User.DisplayNameMinimumLength}");

		_ = RuleFor(x => x.DisplayName)
			.MaximumLength(User.DisplayNameMaximumLength)
			.WithErrorCode($"{nameof(ModifyOwnUserRequestBody.DisplayName)} is too long")
			.WithMessage($"{nameof(ModifyOwnUserRequestBody.DisplayName)} length must be at most {User.DisplayNameMaximumLength}");

		_ = RuleFor(x => x.Description)
			.MinimumLength(User.DescriptionMinimumLength)
			.WithErrorCode($"{nameof(ModifyOwnUserRequestBody.Description)} is too long")
			.WithMessage($"{nameof(ModifyOwnUserRequestBody.Description)} length must be at most {User.DisplayNameMaximumLength}");

		_ = RuleFor(x => x.Description)
			.MaximumLength(User.DescriptionMaximumLength)
			.WithErrorCode($"{nameof(ModifyOwnUserRequestBody.Description)} is too long")
			.WithMessage($"{nameof(ModifyOwnUserRequestBody.Description)} length must be at most {User.DisplayNameMaximumLength}");
	}
}
