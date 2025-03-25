namespace Aula.Server.Common.Identity;

/// <summary>
///     Represents the outcome of a password reset operation.
/// </summary>
internal sealed class ResetPasswordResult
{
	private ResetPasswordResult(String description, Boolean succeeded)
	{
		Description = description;
		Succeeded = succeeded;
	}

	/// <summary>
	///     The operation succeeded.
	/// </summary>
	internal static ResetPasswordResult Success { get; } = new("The operation succeeded", true);

	/// <summary>
	///     An unknown problem has occurred during the operation.
	/// </summary>
	internal static ResetPasswordResult UnknownProblem { get; } = new("An unknown problem has occurred during the operation.", false);

	/// <summary>
	///     The token provided is invalid.
	/// </summary>
	internal static ResetPasswordResult InvalidToken { get; } = new("The token provided is invalid.", false);

	/// <summary>
	///     The password is missing an uppercase character.
	/// </summary>
	internal static ResetPasswordResult MissingUppercaseCharacter { get; } = new("The password is missing an uppercase character.", false);

	/// <summary>
	///     The password is missing a lowercase character.
	/// </summary>
	internal static ResetPasswordResult MissingLowercaseCharacter { get; } = new("The password is missing a lowercase character.", false);

	/// <summary>
	///     The password is missing a digit.
	/// </summary>
	internal static ResetPasswordResult MissingDigit { get; } = new("The password is missing a digit.", false);

	/// <summary>
	///     The password length is invalid.
	/// </summary>
	internal static ResetPasswordResult InvalidLength { get; } = new("The password length is invalid.", false);

	/// <summary>
	///     The password is missing a non-alphanumeric character
	/// </summary>
	internal static ResetPasswordResult MissingNonAlphanumericCharacter { get; } =
		new("The password is missing a non-alphanumeric character", false);

	/// <summary>
	///     The password has not enough unique characters.
	/// </summary>
	internal static ResetPasswordResult NotEnoughUniqueCharacters { get; } = new("The password has not enough unique characters.", false);

	internal String Description { get; }

	/// <summary>
	///     Whether the operation succeeded.
	/// </summary>
	internal Boolean Succeeded { get; private set; }
}
