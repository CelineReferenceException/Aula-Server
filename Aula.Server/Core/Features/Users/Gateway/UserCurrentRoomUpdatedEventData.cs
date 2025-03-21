namespace Aula.Server.Core.Features.Users.Gateway;

/// <summary>
///     Occurs when a user updates its current room.
/// </summary>
internal sealed record UserCurrentRoomUpdatedEventData
{
	/// <summary>
	///     The ID of the user.
	/// </summary>
	public required UInt64 UserId { get; init; }

	/// <summary>
	///     The previous room where the user was in.
	/// </summary>
	public UInt64? PreviousRoomId { get; init; }

	/// <summary>
	///     The new room where the user resides.
	/// </summary>
	public UInt64? CurrentRoomId { get; init; }
}
