namespace Aula.Server.Core.Features.Rooms;

/// <summary>
///     Represents a room within the application.
/// </summary>
internal sealed record RoomData
{
	/// <summary>
	///     The unique identifier of the room.
	/// </summary>
	public required Snowflake Id { get; init; }

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
	public required Boolean IsEntrance { get; init; }

	/// <summary>
	///     A collection of ids of all rooms connected with this room.
	/// </summary>
	public required IReadOnlyList<Snowflake> ConnectedRoomIds { get; init; }

	/// <summary>
	///     The date and time when the room was created.
	/// </summary>
	public required DateTime CreationDate { get; init; }
}
