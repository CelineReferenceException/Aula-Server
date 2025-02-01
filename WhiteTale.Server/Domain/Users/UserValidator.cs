using FluentValidation;

namespace WhiteTale.Server.Domain.Users;

internal sealed class UserValidator : AbstractValidator<User>
{
	public UserValidator()
	{
		_ = RuleFor(x => x.UserName)
			.NotEmpty()
			.MinimumLength(User.UserNameMinimumLength)
			.MaximumLength(User.UserNameMaximumLength);

		_ = RuleFor(x => x.DisplayName)
			.MinimumLength(User.DisplayNameMinimumLength)
			.MaximumLength(User.DisplayNameMaximumLength);

		_ = RuleFor(x => x.Description)
			.MinimumLength(User.DescriptionMinimumLength)
			.MaximumLength(User.DescriptionMaximumLength);

		_ = RuleFor(x => x.Permissions).IsInEnum();
		_ = RuleFor(x => x.OwnerType).IsInEnum();
		_ = RuleFor(x => x.Presence).IsInEnum();
	}
}
