namespace WhiteTale.Server.Features.Rooms.Endpoints;

/// <summary>
///     Holds the data required to remove a connection from this room.
/// </summary>
internal sealed record RemoveRoomConnectionRequestBody
{
	/// <summary>
	///     The ID of the target room to disconnect from this room.
	/// </summary>
	public required UInt64 RoomId { get; init; }
}
