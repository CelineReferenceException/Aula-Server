namespace WhiteTale.Server.Features.Identity;

/// <summary>
///     Holds the data required to register a new user.
/// </summary>
internal sealed class RegisterRequestBody
{
	private readonly String? _displayName;

	private readonly String? _email;

	private readonly String? _userName;

	/// <summary>
	///     The display name for this user. Defaults to the <see cref="UserName">userName</see>.
	/// </summary>
	public String? DisplayName
	{
		get => _displayName;
		init => _displayName = value?.Trim();
	}

	/// <summary>
	///     A unique identifier for the user, required for signing in. Once set it cannot be modified.
	/// </summary>
	public required String UserName
	{
		get => _userName!;
		init => _userName = value.Trim();
	}

	/// <summary>
	///     The email address for the user.
	/// </summary>
	public required String Email
	{
		get => _email!;
		init => _email = value.Trim();
	}

	/// <summary>
	///     The password for the user.
	/// </summary>
	public required String Password { get; init; }
}
