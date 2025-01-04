namespace WhiteTale.Server.Domain.Messages;

/// <summary>
///     Defines the targets of a message.
/// </summary>
internal enum MessageTarget
{
	/// <summary>
	///     Targets a single specific room.
	/// </summary>
	Room = 0,

	/// <summary>
	///     Targets all available rooms.
	/// </summary>
	AllRooms = 1
}
