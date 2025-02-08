namespace WhiteTale.Server.Common.Identity;

/// <summary>
///     User identity related configurations.
/// </summary>
internal sealed class UserOptions
{
	internal const String SectionName = "User";

	/// <inheritdoc cref="UserSignInOptions" />
	public UserSignInOptions SignIn { get; set; } = new();

	/// <inheritdoc cref="UserPasswordOptions" />
	public UserPasswordOptions Password { get; set; } = new();

	/// <inheritdoc cref="UserLockoutOptions" />
	public UserLockoutOptions Lockout { get; set; } = new();
}
