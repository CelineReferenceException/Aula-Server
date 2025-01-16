#pragma warning disable CS8618
namespace WhiteTale.Server.Domain.Rooms;

internal sealed class Room : DomainEntity
{
	internal const Int32 NameMinimumLength = 3;
	internal const Int32 NameMaximumLength = 32;
	internal const Int32 DescriptionMinimumLength = 1;
	internal const Int32 DescriptionMaximumLength = 2048;

	private Room()
	{
	}

	internal UInt64 Id { get; private init; }

	internal String Name { get; private set; }

	internal String? Description { get; private set; }

	internal Boolean IsEntrance { get; private set; }

	internal String ConcurrencyStamp { get; private set; }

	internal Boolean IsRemoved { get; private set; }

	internal static Room Create(UInt64 id, String name, String description, Boolean isEntrance)
	{
		var room = new Room
		{
			Id = id,
			Name = name,
			Description = description,
			IsEntrance = isEntrance,
			ConcurrencyStamp = Guid.NewGuid().ToString("N")
		};

		room.AddEvent(new RoomCreatedEvent(room));

		return room;
	}

	internal void Modify(String? name = null, String? description = null, Boolean? isEntrance = null)
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

		ConcurrencyStamp = Guid.NewGuid().ToString("N");
		AddEvent(new RoomUpdatedEvent(this));
	}

	internal void Remove()
	{
		IsRemoved = true;
		AddEvent(new RoomRemovedEvent(this));
	}
}
