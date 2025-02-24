using FluentValidation;

namespace Aula.Server.Features.Rooms.Endpoints;

internal sealed class ModifyRoomRequestBodyValidator : AbstractValidator<ModifyRoomRequestBody>
{
	public ModifyRoomRequestBodyValidator()
	{
		_ = RuleFor(x => x.Name)
			.MinimumLength(Room.NameMinimumLength)
			.WithErrorCode($"{nameof(ModifyRoomRequestBody.Name)} is too short")
			.WithMessage($"{nameof(ModifyRoomRequestBody.Name)} length must be at least {Room.NameMinimumLength}");

		_ = RuleFor(x => x.Name)
			.MaximumLength(Room.NameMaximumLength)
			.WithErrorCode($"{nameof(ModifyRoomRequestBody.Name)} is too long")
			.WithMessage($"{nameof(ModifyRoomRequestBody.Name)} length must be at most {Room.NameMaximumLength}");

		_ = RuleFor(x => x.Description)
			.MaximumLength(Room.DescriptionMaximumLength)
			.WithErrorCode($"{nameof(ModifyRoomRequestBody.Description)} is too long")
			.WithMessage($"{nameof(ModifyRoomRequestBody.Description)} length must be at most {Room.DescriptionMaximumLength}");
	}
}
