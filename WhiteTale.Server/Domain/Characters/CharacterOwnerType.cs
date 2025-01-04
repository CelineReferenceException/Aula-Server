namespace WhiteTale.Server.Domain.Characters;

/// <summary>
///     Defines the owner types that a character can have.
/// </summary>
internal enum CharacterOwnerType
{
	/// <summary>
	///     The character is owned by a standard user.
	/// </summary>
	Standard = 0,

	/// <summary>
	///     The character is owned by a bot user.
	/// </summary>
	Bot = 1
}
