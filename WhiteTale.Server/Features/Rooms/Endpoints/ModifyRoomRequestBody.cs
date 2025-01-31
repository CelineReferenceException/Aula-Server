namespace WhiteTale.Server.Features.Rooms.Endpoints;

/// <summary>
///     Holds the data required to update an already existing room.
/// </summary>
internal sealed record ModifyRoomRequestBody
{
	/// <summary>
	///     The new name for the room.
	/// </summary>
	public String? Name { get; init; }

	/// <summary>
	///     The new description for the room.
	/// </summary>
	public String? Description { get; init; }

	/// <summary>
	///     Indicates whether the room serves as an entry point for users without an established current room.
	/// </summary>
	public Boolean? IsEntrance { get; init; }
}
