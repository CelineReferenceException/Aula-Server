namespace WhiteTale.Server.Domain.Users;

/// <summary>
///     Enumerates permissions that can be assigned to a user within the application.
/// </summary>
[Flags]
internal enum Permissions
{
	/// <summary>
	///     Grants the user privileges over the entire application.
	/// </summary>
	Administrator = 1 << 0,

	/// <summary>
	///	Allows to create, modify and remove rooms.
	/// </summary>
	ManageRooms = 1 << 1
}
