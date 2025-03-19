namespace Aula.Server.Common.Identity;

/// <summary>
///     Represents the outcome of a register operation.
/// </summary>
internal sealed class RegisterUserResult
{
	private readonly String _name;

	private RegisterUserResult(String name, Boolean succeeded)
	{
		_name = name;
		Succeeded = succeeded;
	}

	/// <summary>
	///     Whether the operation succeeded.
	/// </summary>
	internal Boolean Succeeded { get; private set; }

	/// <summary>
	///     The operation succeeded.
	/// </summary>
	internal static RegisterUserResult Success { get; } = new(nameof(Success), true);

	/// <summary>
	///     The email provided is already in use.
	/// </summary>
	internal static RegisterUserResult EmailInUse { get; } = new(nameof(EmailInUse), false);


	/// <summary>
	///     The username provided is already in use.
	/// </summary>
	internal static RegisterUserResult UserNameInUse { get; } = new(nameof(UserNameInUse), false);

	/// <summary>
	///     The username provided contains invalid characters.
	/// </summary>
	internal static RegisterUserResult InvalidUserNameCharacter { get; } = new(nameof(InvalidUserNameCharacter), false);

	/// <inheritdoc />
	public override String ToString()
	{
		return _name;
	}
}
