namespace Aula.Server.Core.Features.Rooms;

/// <summary>
///     Holds the data required to add connections to this room.
/// </summary>
internal sealed record SetRoomConnectionsRequestBody
{
	/// <summary>
	///     A collection of Ids of the target rooms to connect this room with.
	/// </summary>
	public required IReadOnlyList<Snowflake> RoomIds { get; init; }
}
