using Aula.Server.Domain.Users;
using FluentValidation;

namespace Aula.Server.Features.Users.Endpoints;

internal sealed class SetPermissionsRequestBodyValidator : AbstractValidator<SetPermissionsRequestBody>
{
	public SetPermissionsRequestBodyValidator()
	{
		_ = RuleFor(x => x.Permissions)
			.IsInEnum()
			.WithErrorCode("Invalid permissions")
			.WithMessage("The permissions contains an invalid value.");

		_ = RuleFor(x => x.Permissions)
			.Must(x => !x.HasFlag(Permissions.Administrator))
			.WithErrorCode("Invalid permissions")
			.WithMessage($"'{nameof(Permissions.Administrator)}' is not allowed to be set.");
	}
}
