namespace WhiteTale.Server.Features.Users;

/// <summary>
///     Holds the data required to update the current user.
/// </summary>
internal sealed record ModifyOwnUserRequestBody
{
	private readonly String? _description;

	private readonly String? _displayName;

	/// <summary>
	///     The name of the user.
	/// </summary>
	public String? DisplayName
	{
		get => _displayName;
		init => _displayName = value?.Trim();
	}

	/// <summary>
	///     The description of the user.
	/// </summary>
	public String? Description
	{
		get => _description;
		init => _description = value?.Trim();
	}
}
