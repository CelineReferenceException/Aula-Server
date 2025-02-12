using Aula.Server.Domain.Users;

namespace Aula.Server.Features.Users.Endpoints;

/// <summary>
///     Holds the data required to set the permissions of a user.
/// </summary>
internal sealed record SetPermissionsRequestBody
{
	/// <summary>
	///     The permissions bit flags to set for the user.
	/// </summary>
	public required Permissions Permissions { get; init; }
}
