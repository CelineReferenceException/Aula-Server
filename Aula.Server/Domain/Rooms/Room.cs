namespace Aula.Server.Domain.Rooms;

internal sealed class Room : DefaultDomainEntity
{
	internal const Int32 NameMinimumLength = 3;
	internal const Int32 NameMaximumLength = 32;
	internal const Int32 DescriptionMaximumLength = 2048;

	internal Snowflake Id { get; }

	internal String Name { get; private set; }

	internal String Description { get; private set; }

	internal Boolean IsEntrance { get; private set; }

	internal String ConcurrencyStamp { get; private set; }

	// Readonly navigation property
	internal IReadOnlyList<RoomConnection> Connections { get; }

	internal DateTime CreationDate { get; }

	internal Boolean IsRemoved { get; private set; }

	internal Room(Snowflake id, String name, String description, Boolean isEntrance)
	{
		switch (name.Length)
		{
			case < NameMinimumLength:
				throw new ArgumentOutOfRangeException(nameof(name),
					$"{nameof(name)} length must be at least {NameMinimumLength}.");
			case > NameMaximumLength:
				throw new ArgumentOutOfRangeException(nameof(name),
					$"{nameof(name)} length must be at most ${NameMaximumLength}.");
			default: break;
		}

		if (description.Length > DescriptionMaximumLength)
		{
			throw new ArgumentOutOfRangeException(nameof(description),
				$"{nameof(description)} length must be at most ${DescriptionMaximumLength}.");
		}

		Id = id;
		Name = name;
		Description = description;
		IsEntrance = isEntrance;
		Connections = [];
		ConcurrencyStamp = Guid.NewGuid().ToString("N");
		CreationDate = DateTime.UtcNow;

		AddEvent(new RoomCreatedEvent(this));
	}

	internal void Modify(
		String? name = null,
		String? description = null,
		Boolean? isEntrance = null)
	{
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
			return;
		}

		AddEvent(new RoomUpdatedEvent(this));
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
		AddEvent(new RoomRemovedEvent(this));
	}

	private static String GenerateConcurrencyStamp()
	{
		return Guid.CreateVersion7().ToString("N");
	}
}
