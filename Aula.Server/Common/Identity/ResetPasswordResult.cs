namespace Aula.Server.Common.Identity;

/// <summary>
///     Represents the outcome of a password reset operation.
/// </summary>
internal sealed class ResetPasswordResult
{
	private readonly String _description;

	private ResetPasswordResult(String description, Boolean succeeded)
	{
		_description = description;
		Succeeded = succeeded;
	}

	/// <summary>
	///     Whether the operation succeeded.
	/// </summary>
	internal Boolean Succeeded { get; private set; }

	/// <summary>
	///     The operation succeeded.
	/// </summary>
	internal static ResetPasswordResult Success { get; } = new(nameof(Success), true);

	/// <summary>
	///     An unknown problem has occurred during the operation.
	/// </summary>
	internal static ResetPasswordResult UnknownProblem { get; } = new(nameof(UnknownProblem), false);

	/// <summary>
	///     The token provided is invalid.
	/// </summary>
	internal static ResetPasswordResult InvalidToken { get; } = new(nameof(InvalidToken), false);

	/// <summary>
	///     The password is missing an uppercase character.
	/// </summary>
	internal static ResetPasswordResult MissingUppercaseCharacter { get; } = new(nameof(MissingUppercaseCharacter), false);

	/// <summary>
	///     The password is missing a lowercase character.
	/// </summary>
	internal static ResetPasswordResult MissingLowercaseCharacter { get; } = new(nameof(MissingLowercaseCharacter), false);

	/// <summary>
	///     The password is missing a digit.
	/// </summary>
	internal static ResetPasswordResult MissingDigit { get; } = new(nameof(MissingDigit), false);

	/// <summary>
	///     The password length is invalid.
	/// </summary>
	internal static ResetPasswordResult InvalidLength { get; } = new(nameof(InvalidLength), false);

	/// <summary>
	///     The password is missing a non-alphanumeric character
	/// </summary>
	internal static ResetPasswordResult MissingNonAlphanumericCharacter { get; } = new(nameof(MissingNonAlphanumericCharacter), false);

	/// <summary>
	///     The password has not enough unique characters.
	/// </summary>
	internal static ResetPasswordResult NotEnoughUniqueCharacters { get; } = new(nameof(NotEnoughUniqueCharacters), false);

	/// <inheritdoc />
	public override String ToString()
	{
		return _description;
	}
}
