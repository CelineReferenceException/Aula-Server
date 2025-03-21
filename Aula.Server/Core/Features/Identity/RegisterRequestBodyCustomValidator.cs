namespace Aula.Server.Core.Features.Identity;

internal sealed class RegisterRequestBodyCustomValidator : IValidator<RegisterRequestBody>
{
	private static readonly ResultProblem s_displayNameTooShort =
		new($"{nameof(RegisterRequestBody.DisplayName)} is too short",
			$"{nameof(RegisterRequestBody.DisplayName)} length must be at least {User.DisplayNameMinimumLength}");

	private static readonly ResultProblem s_displayNameTooLong =
		new($"{nameof(RegisterRequestBody.DisplayName)} is too long",
			$"{nameof(RegisterRequestBody.DisplayName)} length must be at most {User.DisplayNameMaximumLength}");

	private static readonly ResultProblem s_userNameTooShort =
		new($"{nameof(RegisterRequestBody.UserName)} is too short",
			$"{nameof(RegisterRequestBody.UserName)} length must be at least {User.UserNameMinimumLength}");

	private static readonly ResultProblem s_userNameTooLong =
		new($"{nameof(RegisterRequestBody.UserName)} is too long",
			$"{nameof(RegisterRequestBody.UserName)} length must be at most {User.UserNameMaximumLength}");

	private static readonly ResultProblem s_invalidEmail =
		new($"Invalid {nameof(RegisterRequestBody.Email)}",
			$"{nameof(RegisterRequestBody.Email)} is not a valid email address.");

	private static readonly ResultProblem s_passwordIsEmpty =
		new($"{nameof(RegisterRequestBody.Password)} is empty",
			$"{nameof(RegisterRequestBody.Password)} cannot be empty.");

	public Result Validate(RegisterRequestBody obj)
	{
		ArgumentNullException.ThrowIfNull(obj);

		var errors = new Items<ResultProblem>();

		if (obj.DisplayName is not null)
		{
			if (obj.DisplayName.Length < User.DisplayNameMinimumLength)
			{
				errors.Add(s_displayNameTooShort);
			}
			else if (obj.DisplayName.Length > User.DisplayNameMaximumLength)
			{
				errors.Add(s_displayNameTooLong);
			}
		}

		if (obj.UserName.Length < User.UserNameMinimumLength)
		{
			errors.Add(s_userNameTooShort);
		}
		else if (obj.UserName.Length > User.UserNameMaximumLength)
		{
			errors.Add(s_userNameTooLong);
		}

		if (false /*TODO: Email regex*/)
		{
			errors.Add(s_invalidEmail);
		}

		if (String.IsNullOrWhiteSpace(obj.Password))
		{
			errors.Add(s_passwordIsEmpty);
		}

		return errors.Count is 0
			? Result.Success
			: new Result(errors.UnderlyingItem is not null
				? new ResultProblemValues(errors.UnderlyingItem)
				: new ResultProblemValues(errors.UnderlyingList!));
	}
}
