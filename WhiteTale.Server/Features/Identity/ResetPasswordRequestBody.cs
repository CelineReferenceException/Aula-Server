namespace WhiteTale.Server.Features.Identity;

/// <summary>
///     Represents the data required to reset a user's password.
/// </summary>
internal sealed class ResetPasswordRequestBody
{
	/// <summary>
	///     The unique identifier of the user.
	/// </summary>
	public required UInt64 UserId { get; init; }

	/// <summary>
	///     The token used to validate the password reset request.
	/// </summary>
	public required String ResetToken { get; init; }

	/// <summary>
	///     The new password to be set for the user.
	/// </summary>
	public required String NewPassword { get; init; }
}
