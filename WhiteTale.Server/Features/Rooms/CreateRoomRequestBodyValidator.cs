using FluentValidation;

namespace WhiteTale.Server.Features.Rooms;

internal sealed class CreateRoomRequestBodyValidator : AbstractValidator<CreateRoomRequestBody>
{
	public CreateRoomRequestBodyValidator()
	{
		_ = RuleFor(x => x.Name)
			.MinimumLength(Room.NameMinimumLength)
			.WithErrorCode($"{nameof(Room.Name)} is too short")
			.WithMessage(
				$"{nameof(Room.Name)} length must be at least {Room.NameMinimumLength}");

		_ = RuleFor(x => x.Name)
			.MaximumLength(Room.NameMaximumLength)
			.WithErrorCode($"{nameof(Room.Name)} is too long")
			.WithMessage(
				$"{nameof(Room.Name)} length must be at most {Room.NameMaximumLength}");

		_ = RuleFor(x => x.Description)
			.MinimumLength(Room.DescriptionMinimumLength)
			.WithErrorCode($"{nameof(Room.Description)} is too short")
			.WithMessage(
				$"{nameof(Room.Description)} length must be at least {Room.DescriptionMinimumLength}");

		_ = RuleFor(x => x.Description)
			.MaximumLength(Room.DescriptionMaximumLength)
			.WithErrorCode($"{nameof(Room.Description)} is too long")
			.WithMessage(
				$"{nameof(Room.Description)} length must be at most {Room.DescriptionMaximumLength}");
	}
}
