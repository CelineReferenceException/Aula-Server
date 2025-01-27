namespace WhiteTale.Server.Features.Users;

/// <summary>
///     Holds the data required to set the current room of a user.
/// </summary>
internal sealed class SetCurrentRoomRequestBody
{
	/// <summary>
	///     The ID of the new current room.
	/// </summary>
	public required UInt64 RoomId { get; init; }
}
