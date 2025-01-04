namespace WhiteTale.Server.Features.Identity;

/// <summary>
///     Represents the data required to authenticate a user.
/// </summary>
internal sealed class LogInRequestBody
{
	/// <summary>
	///     The unique identifier of the user to authenticate.
	/// </summary>
	public required String UserName { get; init; }

	/// <summary>
	///     The password of the user.
	/// </summary>
	public required String Password { get; init; }
}
