using FluentValidation;

namespace Aula.Server.Core.Domain.Users;

internal sealed class UserValidator : AbstractValidator<User>
{
	public UserValidator()
	{
		_ = RuleFor(x => x.UserName).NotEmpty();
		_ = RuleFor(x => x.UserName).MinimumLength(User.UserNameMinimumLength);
		_ = RuleFor(x => x.UserName).MaximumLength(User.UserNameMaximumLength);
		_ = RuleFor(x => x.DisplayName).MinimumLength(User.DisplayNameMinimumLength);
		_ = RuleFor(x => x.DisplayName).MaximumLength(User.DisplayNameMaximumLength);
		_ = RuleFor(x => x.Description).MaximumLength(User.DescriptionMaximumLength);
		_ = RuleFor(x => x.Permissions).IsInEnum();
		_ = RuleFor(x => x.Type).IsInEnum();
		_ = RuleFor(x => x.Presence).IsInEnum();
		_ = When(x => x.Type is UserType.Standard, () => { _ = RuleFor(x => x.Email).EmailAddress(); });
	}

	internal static UserValidator Instance { get; } = new();
}
