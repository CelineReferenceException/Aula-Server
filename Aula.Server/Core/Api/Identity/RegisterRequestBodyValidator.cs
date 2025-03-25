using Aula.Server.Core.Domain.Users;
using Aula.Server.Core.Identity;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace Aula.Server.Core.Api.Identity;

internal sealed class RegisterRequestBodyValidator : AbstractValidator<RegisterRequestBody>
{
	public RegisterRequestBodyValidator(IOptions<IdentityOptions> options)
	{
		_ = RuleFor(x => x.DisplayName)
			.MinimumLength(User.DisplayNameMinimumLength)
			.WithErrorCode(nameof(RegisterRequestBody.DisplayName).ToCamel())
			.WithMessage($"Length must be at least {User.DisplayNameMinimumLength}");

		_ = RuleFor(x => x.DisplayName)
			.MaximumLength(User.DisplayNameMaximumLength)
			.WithErrorCode(nameof(RegisterRequestBody.DisplayName).ToCamel())
			.WithMessage($"Length must be at most {User.DisplayNameMaximumLength}");

		_ = RuleFor(x => x.UserName)
			.MinimumLength(User.UserNameMinimumLength)
			.WithErrorCode(nameof(RegisterRequestBody.UserName).ToCamel())
			.WithMessage($"Length must be at least {User.DisplayNameMinimumLength}");

		_ = RuleFor(x => x.UserName)
			.MaximumLength(User.UserNameMaximumLength)
			.WithErrorCode(nameof(RegisterRequestBody.UserName).ToCamel())
			.WithMessage($"Length must be at most {User.DisplayNameMaximumLength}");

		_ = RuleFor(x => x.Email)
			.EmailAddress()
			.WithErrorCode(nameof(RegisterRequestBody.Email).ToCamel())
			.WithMessage($"Must be a valid email address");

		_ = RuleFor(x => x.Password)
			.MinimumLength(options.Value.Password.RequiredLength)
			.WithErrorCode(nameof(RegisterRequestBody.Password).ToCamel())
			.WithMessage($"Length must be at least {options.Value.Password.RequiredLength}");

		_ = RuleFor(x => x.Password)
			.MaximumLength(User.PasswordMaximumLength)
			.WithErrorCode(nameof(RegisterRequestBody.Password).ToCamel())
			.WithMessage($"Length must be at most {User.PasswordMaximumLength}");
	}
}
