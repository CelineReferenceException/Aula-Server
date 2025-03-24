using FluentValidation;

namespace Aula.Server.Domain.Rooms;

internal sealed class RoomValidator : AbstractValidator<Room>
{
	public RoomValidator()
	{
		_ = RuleFor(x => x.Name)
			.NotNull()
			.WithErrorCode(nameof(Room.Name))
			.WithMessage("Required");

		_ = RuleFor(x => x.Name)
			.MinimumLength(Room.NameMinimumLength)
			.WithErrorCode(nameof(Room.Name))
			.WithMessage($"Length must be at least {Room.NameMinimumLength}.");


		_ = RuleFor(x => x.Name)
			.MaximumLength(Room.NameMaximumLength)
			.WithErrorCode(nameof(Room.Name))
			.WithMessage($"Length must be at most {Room.NameMaximumLength}.");

		_ = RuleFor(x => x.Description)
			.NotNull()
			.WithErrorCode(nameof(Room.Description))
			.WithMessage("Required");

		_ = RuleFor(x => x.Description)
			.MaximumLength(Room.DescriptionMaximumLength)
			.WithErrorCode(nameof(Room.Description))
			.WithMessage($"Length must be at most {Room.DescriptionMaximumLength}.");
	}
}
