using Aula.Server.Domain.Messages;

namespace Aula.Server.Features.Messages;

/// <summary>
///     Holds data required by <see cref="MessageType.UserLeave" /> messages.
/// </summary>
internal sealed record MessageUserLeaveData
{
	/// <summary>
	///     The ID of user who moved out of the room.
	/// </summary>
	public required UInt64 UserId { get; init; }

	/// <summary>
	///     The ID of the room where it resides now.
	/// </summary>
	public required UInt64? RoomId { get; init; }
}
