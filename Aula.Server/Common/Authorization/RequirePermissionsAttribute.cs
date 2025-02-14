namespace Aula.Server.Common.Authorization;

/// <summary>
///     Indicates the permissions required by an endpoint.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
internal sealed class RequirePermissionsAttribute : Attribute
{
	internal RequirePermissionsAttribute(params IReadOnlyList<Permissions> requiredPermissions)
	{
		RequiredPermissions = requiredPermissions;
	}

	public IReadOnlyList<Permissions> RequiredPermissions { get; }
}
