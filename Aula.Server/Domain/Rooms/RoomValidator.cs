using FluentValidation;

namespace Aula.Server.Domain.Rooms;

internal sealed class RoomValidator : AbstractValidator<Room>
{
	public RoomValidator()
	{
		_ = RuleFor(x => x.Name).NotNull();
		_ = RuleFor(x => x.Name).MinimumLength(Room.NameMinimumLength);
		_ = RuleFor(x => x.Name).MaximumLength(Room.NameMaximumLength);
		_ = RuleFor(x => x.Description).NotNull();
		_ = RuleFor(x => x.Description).MaximumLength(Room.DescriptionMaximumLength);
	}

	internal static RoomValidator Instance { get; } = new();
}
