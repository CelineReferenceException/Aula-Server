namespace WhiteTale.Server.Common.Authentication;

internal sealed class ResetPasswordResult
{
	private readonly String _name;

	internal Boolean Succeeded { get; private set; }

	private ResetPasswordResult(String name, Boolean succeeded)
	{
		_name = name;
		Succeeded = succeeded;
	}

	public override String ToString()
	{
		return _name;
	}

	internal static ResetPasswordResult Success { get; } = new(nameof(Success), true);

	internal static ResetPasswordResult UnknownProblem { get; } = new(nameof(UnknownProblem), false);

	internal static ResetPasswordResult InvalidToken { get; } = new(nameof(InvalidToken), false);

	internal static ResetPasswordResult MissingUppercaseCharacter { get; } = new(nameof(MissingUppercaseCharacter), false);

	internal static ResetPasswordResult MissingLowercaseCharacter { get; } = new(nameof(MissingLowercaseCharacter), false);

	internal static ResetPasswordResult MissingDigit { get; } = new(nameof(MissingDigit), false);

	internal static ResetPasswordResult InvalidLength { get; } = new(nameof(InvalidLength), false);

	internal static ResetPasswordResult MissingNonAlphanumericCharacter { get; } = new(nameof(MissingNonAlphanumericCharacter), false);

	internal static ResetPasswordResult NotEnoughUniqueCharacters { get; } = new(nameof(NotEnoughUniqueCharacters), false);
}
