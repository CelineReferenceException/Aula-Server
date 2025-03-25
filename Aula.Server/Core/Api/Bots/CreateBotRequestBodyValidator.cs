using Aula.Server.Core.Domain.Users;
using FluentValidation;

namespace Aula.Server.Core.Api.Bots;

internal sealed class CreateBotRequestBodyValidator : AbstractValidator<CreateBotRequestBody>
{
	public CreateBotRequestBodyValidator()
	{
		_ = RuleFor(x => x.DisplayName)
			.MinimumLength(User.DisplayNameMinimumLength)
			.WithErrorCode(nameof(CreateBotRequestBody.DisplayName).ToCamel())
			.WithMessage($"Length must be at least {User.DisplayNameMinimumLength}");

		_ = RuleFor(x => x.DisplayName)
			.MaximumLength(User.DisplayNameMaximumLength)
			.WithErrorCode(nameof(CreateBotRequestBody.DisplayName).ToCamel())
			.WithMessage($"Length must be at most {User.DisplayNameMaximumLength}");
	}
}
