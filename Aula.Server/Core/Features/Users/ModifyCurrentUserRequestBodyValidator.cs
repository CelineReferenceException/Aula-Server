﻿using FluentValidation;

namespace Aula.Server.Core.Features.Users;

internal sealed class ModifyCurrentUserRequestBodyValidator : AbstractValidator<ModifyCurrentUserRequestBody>
{
	public ModifyCurrentUserRequestBodyValidator()
	{
		_ = RuleFor(x => x.DisplayName)
			.MinimumLength(User.DisplayNameMinimumLength)
			.WithErrorCode($"{nameof(ModifyCurrentUserRequestBody.DisplayName)} is too short")
			.WithMessage($"{nameof(ModifyCurrentUserRequestBody.DisplayName)} length must be at least {User.DisplayNameMinimumLength}");

		_ = RuleFor(x => x.DisplayName)
			.MaximumLength(User.DisplayNameMaximumLength)
			.WithErrorCode($"{nameof(ModifyCurrentUserRequestBody.DisplayName)} is too long")
			.WithMessage($"{nameof(ModifyCurrentUserRequestBody.DisplayName)} length must be at most {User.DisplayNameMaximumLength}");

		_ = RuleFor(x => x.Description)
			.MaximumLength(User.DescriptionMaximumLength)
			.WithErrorCode($"{nameof(ModifyCurrentUserRequestBody.Description)} is too long")
			.WithMessage($"{nameof(ModifyCurrentUserRequestBody.Description)} length must be at most {User.DescriptionMaximumLength}");
	}
}
