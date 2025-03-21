using FluentValidation;

namespace Aula.Server.Core.Features.Rooms.Endpoints;

internal sealed class CreateRoomRequestBodyValidator : AbstractValidator<CreateRoomRequestBody>
{
	public CreateRoomRequestBodyValidator()
	{
		_ = RuleFor(x => x.Name)
			.MinimumLength(Room.NameMinimumLength)
			.WithErrorCode($"{nameof(CreateRoomRequestBody.Name)} is too short")
			.WithMessage($"{nameof(CreateRoomRequestBody.Name)} length must be at least {Room.NameMinimumLength}");

		_ = RuleFor(x => x.Name)
			.MaximumLength(Room.NameMaximumLength)
			.WithErrorCode($"{nameof(CreateRoomRequestBody.Name)} is too long")
			.WithMessage($"{nameof(CreateRoomRequestBody.Name)} length must be at most {Room.NameMaximumLength}");

		_ = RuleFor(x => x.Description)
			.MaximumLength(Room.DescriptionMaximumLength)
			.WithErrorCode($"{nameof(CreateRoomRequestBody.Description)} is too long")
			.WithMessage($"{nameof(CreateRoomRequestBody.Description)} length must be at most {Room.DescriptionMaximumLength}");
	}
}
