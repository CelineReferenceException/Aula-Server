using FluentValidation;

namespace Aula.Server.Core.Features.Bots.Endpoints;

internal sealed class CreateBotRequestBodyValidator : AbstractValidator<CreateBotRequestBody>
{
	public CreateBotRequestBodyValidator()
	{
		_ = RuleFor(x => x.DisplayName)
			.MinimumLength(User.DisplayNameMinimumLength)
			.WithErrorCode($"{nameof(CreateBotRequestBody.DisplayName)} is too short")
			.WithMessage($"{nameof(CreateBotRequestBody.DisplayName)} length must be at least {User.DisplayNameMinimumLength}");

		_ = RuleFor(x => x.DisplayName)
			.MaximumLength(User.DisplayNameMaximumLength)
			.WithErrorCode($"{nameof(CreateBotRequestBody.DisplayName)} is too long")
			.WithMessage($"{nameof(CreateBotRequestBody.DisplayName)} length must be at most {User.DisplayNameMaximumLength}");
	}
}
