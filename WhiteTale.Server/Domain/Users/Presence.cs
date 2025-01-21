namespace WhiteTale.Server.Domain.Users;

/// <summary>
///     Defines presence statuses for a character.
/// </summary>
internal enum Presence
{
	/// <summary>
	///     The character is offline.
	/// </summary>
	Offline = 0,

	/// <summary>
	///     The character is online.
	/// </summary>
	Online = 1,
}
