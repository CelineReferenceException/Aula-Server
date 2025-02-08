namespace WhiteTale.Server.Common.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
internal sealed class RequirePermissionsAttribute : Attribute
{
	internal RequirePermissionsAttribute(params Permissions[] requiredPermissions)
	{
		RequiredPermissions = requiredPermissions;
	}

	public IReadOnlyList<Permissions> RequiredPermissions { get; }
}
