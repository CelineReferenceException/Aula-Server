namespace WhiteTale.Server.Features.Rooms.Connections;

/// <summary>
///     Holds the data required to add a connection to this room.
/// </summary>
internal sealed class AddConnectionRequestBody
{
	/// <summary>
	///     The ID of the target room to connect this room with.
	/// </summary>
	public required UInt64 RoomId { get; init; }
}
