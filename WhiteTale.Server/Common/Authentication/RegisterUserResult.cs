namespace WhiteTale.Server.Common.Authentication;

internal sealed class RegisterUserResult
{
	private readonly String _name;

	private RegisterUserResult(String name, Boolean succeeded)
	{
		_name = name;
		Succeeded = succeeded;
	}

	internal Boolean Succeeded { get; private set; }

	internal static RegisterUserResult Success { get; } = new(nameof(Success), true);

	internal static RegisterUserResult EmailInUse { get; } = new(nameof(EmailInUse), false);

	internal static RegisterUserResult UserNameInUse { get; } = new(nameof(UserNameInUse), false);

	internal static RegisterUserResult InvalidUserNameCharacter { get; } = new(nameof(InvalidUserNameCharacter), false);

	public override String ToString()
	{
		return _name;
	}
}
