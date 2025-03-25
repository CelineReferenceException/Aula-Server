using FluentValidation.Results;

namespace Aula.Server.Domain.Rooms;

internal sealed class Room : DefaultDomainEntity
{
	internal const Int32 NameMinimumLength = 3;
	internal const Int32 NameMaximumLength = 32;
	internal const Int32 DescriptionMaximumLength = 2048;

	private Room(
		Snowflake id,
		String name,
		String description,
		Boolean isEntrance,
		String concurrencyStamp,
		DateTime creationDate,
		Boolean isRemoved)
	{
		Id = id;
		Name = name;
		Description = description;
		IsEntrance = isEntrance;
		ConcurrencyStamp = concurrencyStamp;
		CreationDate = creationDate;
		IsRemoved = isRemoved;
	}

	internal Snowflake Id { get; }

	internal String Name { get; private set; }

	internal String Description { get; private set; }

	internal Boolean IsEntrance { get; private set; }

	internal String ConcurrencyStamp { get; private set; }

	// Readonly navigation property
	internal IReadOnlyList<RoomConnection> Connections { get; private init; }

	internal DateTime CreationDate { get; }

	internal Boolean IsRemoved { get; private set; }

	internal static Result<Room, ValidationFailure> Create(Snowflake id, String name, String description, Boolean isEntrance)
	{
		var room = new Room(id, name, description, isEntrance, GenerateConcurrencyStamp(), DateTime.UtcNow, false)
		{
			Connections = [],
		};
		room.Events.Add(new RoomCreatedEvent(room));

		var validationResult = RoomValidator.Instance.Validate(room);
		return validationResult.IsValid
			? room
			: new ResultErrorValues<ValidationFailure>(new Items<ValidationFailure>(validationResult.Errors));
	}

	internal Result<ValidationFailure> Modify(
		String? name = null,
		String? description = null,
		Boolean? isEntrance = null)
	{
		var oldName = Name;
		var oldDescription = Description;
		var oldIsEntrance = IsEntrance;
		var modified = false;

		if (name is not null &&
		    name != Name)
		{
			Name = name;
			modified = true;
		}

		if (description is not null &&
		    description != Description)
		{
			Description = description;
			modified = true;
		}

		if (isEntrance is not null &&
		    isEntrance != IsEntrance)
		{
			IsEntrance = (Boolean)isEntrance;
			modified = true;
		}

		if (!modified)
		{
			return Result<ValidationFailure>.Success;
		}

		var validationResult = RoomValidator.Instance.Validate(this);
		if (!validationResult.IsValid)
		{
			Name = oldName;
			Description = oldDescription;
			IsEntrance = oldIsEntrance;

			return new ResultErrorValues<ValidationFailure>(validationResult.Errors);
		}

		if (Events.All(e => e is not RoomUpdatedEvent))
		{
			Events.Add(new RoomUpdatedEvent(this));
		}

		return Result<ValidationFailure>.Success;
	}

	internal void UpdateConcurrencyStamp()
	{
		ConcurrencyStamp = GenerateConcurrencyStamp();
	}

	internal void Remove()
	{
		if (IsRemoved)
		{
			return;
		}

		IsRemoved = true;
		Events.Add(new RoomRemovedEvent(this));
	}

	private static String GenerateConcurrencyStamp()
	{
		return Guid.CreateVersion7().ToString("N");
	}
}
