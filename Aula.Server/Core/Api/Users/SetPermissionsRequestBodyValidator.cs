using Aula.Server.Core.Domain.Users;
using FluentValidation;

namespace Aula.Server.Core.Api.Users;

internal sealed class SetPermissionsRequestBodyValidator : AbstractValidator<SetPermissionsRequestBody>
{
	public SetPermissionsRequestBodyValidator()
	{
		var disallowedPermissions = new[] { Permissions.Administrator, };
		var allowedPermissions = Enum.GetValues<Permissions>().Except(disallowedPermissions);

		_ = RuleFor(x => x.Permissions)
			.IsInEnum()
			.Must(v => disallowedPermissions.All(disallowedPermission => !v.HasFlag(disallowedPermission)))
			.WithErrorCode(nameof(Permissions).ToCamel())
			.WithMessage("Invalid value." +
			             $"Valid values are combinations of the following flags: {String.Join(", ", allowedPermissions.Cast<UInt64>())}");
	}
}
