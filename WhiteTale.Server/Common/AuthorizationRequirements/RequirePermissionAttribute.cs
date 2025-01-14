using WhiteTale.Server.Domain.Users;

namespace WhiteTale.Server.Common.AuthorizationRequirements;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
internal sealed class RequirePermissionAttribute : Attribute
{
	internal RequirePermissionAttribute(params Permissions[] requiredPermissions)
	{
		if (requiredPermissions.Length < 1)
		{
			throw new ArgumentOutOfRangeException(nameof(requiredPermissions), $"At least one '{nameof(Permissions)}' must be assigned");
		}

		RequiredPermissions = requiredPermissions;
	}

	public IReadOnlyList<Permissions> RequiredPermissions { get; }
}
