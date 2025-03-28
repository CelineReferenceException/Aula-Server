﻿using Aula.Server.Domain.Rooms;
using FluentValidation;

namespace Aula.Server.Core.Api.Rooms;

internal sealed class CreateRoomRequestBodyValidator : AbstractValidator<CreateRoomRequestBody>
{
	public CreateRoomRequestBodyValidator()
	{
		_ = RuleFor(x => x.Name)
			.MinimumLength(Room.NameMinimumLength)
			.WithErrorCode(nameof(CreateRoomRequestBody.Name).ToCamel())
			.WithMessage($"Length must be at least {Room.NameMinimumLength}");

		_ = RuleFor(x => x.Name)
			.MaximumLength(Room.NameMaximumLength)
			.WithErrorCode(nameof(CreateRoomRequestBody.Name).ToCamel())
			.WithMessage($"Length must be at most {Room.NameMaximumLength}");

		_ = RuleFor(x => x.Description)
			.MaximumLength(Room.DescriptionMaximumLength)
			.WithErrorCode(nameof(CreateRoomRequestBody.Description).ToCamel())
			.WithMessage($"Length must be at most {Room.DescriptionMaximumLength}");
	}
}
