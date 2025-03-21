namespace Aula.Server.Core.Features.Users.Gateway;

/// <summary>
///     Occurs when a user updates its current room.
/// </summary>
internal sealed record UserCurrentRoomUpdatedEventData
{
	/// <summary>
	///     The ID of the user.
	/// </summary>
	public required Snowflake UserId { get; init; }

	/// <summary>
	///     The previous room where the user was in.
	/// </summary>
	public Snowflake? PreviousRoomId { get; init; }

	/// <summary>
	///     The new room where the user resides.
	/// </summary>
	public Snowflake? CurrentRoomId { get; init; }
}
