using Aula.Server.Core.Domain;

namespace Aula.Server.Core.Features.Users;

/// <summary>
///     Holds the data required to set the current room of a user.
/// </summary>
internal sealed record SetUserRoomRequestBody
{
	/// <summary>
	///     The ID of the new current room.
	/// </summary>
	public required Snowflake RoomId { get; init; }
}
