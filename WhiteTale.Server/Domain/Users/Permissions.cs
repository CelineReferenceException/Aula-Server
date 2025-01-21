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
	///     Allows to create, modify and remove rooms.
	/// </summary>
	ManageRooms = 1 << 1,

	/// <summary>
	///     Allows retrieving previously sent messages.
	/// </summary>
	ReadMessages = 1 << 2,

	/// <summary>
	///     Allows to send messages.
	/// </summary>
	SendMessages = 1 << 3,

	/// <summary>
	///     Allows to delete messages sent by other users.
	/// </summary>
	ManageMessages = 1 << 4,

	/// <summary>
	///     Allows the user to set their current room.
	/// </summary>
	SetOwnCurrentRoom = 1 << 5,

	/// <summary>
	///     Allows to set the current room of any user.
	/// </summary>
	SetCurrentRoom = 1 << 6,
}
