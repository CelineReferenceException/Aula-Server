using Aula.Server.Domain.Users;

namespace Aula.Server.Common.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
internal sealed class RequirePermissionsAttribute : Attribute
{
	internal RequirePermissionsAttribute(params IReadOnlyList<Permissions> requiredPermissions)
	{
		RequiredPermissions = requiredPermissions;
	}

	public IReadOnlyList<Permissions> RequiredPermissions { get; }
}
