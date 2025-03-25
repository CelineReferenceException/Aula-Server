using Aula.Server.Core.Domain;

namespace Aula.Server.Core.Features.Rooms;

/// <summary>
///     Represents a connection between two rooms.
/// </summary>
internal sealed record RoomConnectionEventData
{
	/// <summary>
	///     The source room from where users come from.
	/// </summary>
	public required Snowflake SourceRoomId { get; init; }

	/// <summary>
	///     The destination room.
	/// </summary>
	public required Snowflake TargetRoomId { get; init; }
}
