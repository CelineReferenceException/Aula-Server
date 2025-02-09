using FluentValidation;

namespace WhiteTale.Server.Features.Bots.Endpoints;

internal sealed class CreateBotRequestBodyValidator : AbstractValidator<CreateBotRequestBody>
{
	public CreateBotRequestBodyValidator()
	{
		_ = RuleFor(x => x.DisplayName)
			.NotEmpty()
			.MinimumLength(User.DisplayNameMinimumLength)
			.MaximumLength(User.DisplayNameMaximumLength);
	}
}
