namespace WhiteTale.Server.Features.Rooms.Connections;

/// <summary>
///     Holds the data required to remove a connection from this room.
/// </summary>
internal sealed class RemoveConnectionRequestBody
{
	/// <summary>
	///     The ID of the target room to disconnect from this room.
	/// </summary>
	public required UInt64 RoomId { get; init; }
}
