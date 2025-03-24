using FluentValidation;

namespace Aula.Server.Domain.Rooms;

internal sealed class RoomValidator : AbstractValidator<Room>
{
	public RoomValidator()
	{
		_ = RuleFor(x => x.Name)
			.NotNull()
			.MinimumLength(Room.NameMinimumLength)
			.MaximumLength(Room.NameMaximumLength);

		_ = RuleFor(x => x.Description)
			.NotNull()
			.MaximumLength(Room.DescriptionMaximumLength);
	}
}
