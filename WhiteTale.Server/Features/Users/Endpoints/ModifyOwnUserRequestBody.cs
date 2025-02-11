namespace WhiteTale.Server.Features.Users.Endpoints;

/// <summary>
///     Holds the data required to update the current user.
/// </summary>
internal sealed record ModifyOwnUserRequestBody
{
	/// <summary>
	///     The name of the user.
	/// </summary>
	public String? DisplayName { get; init; }

	/// <summary>
	///     The description of the user.
	/// </summary>
	public String? Description { get; init; }
}
