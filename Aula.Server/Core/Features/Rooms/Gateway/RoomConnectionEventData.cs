namespace Aula.Server.Core.Features.Rooms.Gateway;

/// <summary>
///     Represents a connection between two rooms.
/// </summary>
internal sealed record RoomConnectionEventData
{
	/// <summary>
	///     The source room from where users come from.
	/// </summary>
	public required UInt64 SourceRoomId { get; init; }

	/// <summary>
	///     The destination room.
	/// </summary>
	public required UInt64 TargetRoomId { get; init; }
}
