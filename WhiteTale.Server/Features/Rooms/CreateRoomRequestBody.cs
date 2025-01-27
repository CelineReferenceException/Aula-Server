namespace WhiteTale.Server.Features.Rooms;

/// <summary>
///     Holds the data required to create a new room.
/// </summary>
internal sealed class CreateRoomRequestBody
{
	/// <summary>
	///     The name of the room.
	/// </summary>
	public required String Name { get; init; }

	/// <summary>
	///     The description of the room.
	/// </summary>
	public String? Description { get; init; }

	/// <summary>
	///     Whether the room serves as an entry point for users without an established current room.
	/// </summary>
	public Boolean? IsEntrance { get; init; }
}
