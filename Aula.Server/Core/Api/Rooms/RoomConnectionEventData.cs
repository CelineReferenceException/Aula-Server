using Aula.Server.Domain;

namespace Aula.Server.Core.Api.Rooms;

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
