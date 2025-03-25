namespace Aula.Server.Core.Api.Rooms;

/// <summary>
///     Holds the data required to create a new room.
/// </summary>
internal sealed record CreateRoomRequestBody
{
	/// <summary>
	///     The name of the room.
	/// </summary>
	public required String Name { get; init; }

	/// <summary>
	///     The description of the room.
	/// </summary>
	public required String Description { get; init; }

	/// <summary>
	///     Whether the room serves as an entry point for users without an established current room.
	/// </summary>
	public Boolean? IsEntrance { get; init; }
}
