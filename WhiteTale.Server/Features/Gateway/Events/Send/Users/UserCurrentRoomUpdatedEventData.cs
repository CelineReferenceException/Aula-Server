namespace WhiteTale.Server.Features.Gateway.Events.Send.Users;

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
	///     The new room where the user resides.
	/// </summary>
	public UInt64? RoomId { get; init; }
}
