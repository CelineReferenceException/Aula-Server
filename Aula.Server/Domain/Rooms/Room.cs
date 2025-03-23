namespace Aula.Server.Domain.Rooms;

internal sealed class Room : DefaultDomainEntity
{
	internal const Int32 NameMinimumLength = 3;
	internal const Int32 NameMaximumLength = 32;
	internal const Int32 DescriptionMaximumLength = 2048;

	private static readonly ResultProblem s_nameTooShort =
		new("Username is too short", $"Username length must be at least {NameMinimumLength}.");

	private static readonly ResultProblem s_nameTooLong =
		new("Username is too long", $"Username length must be at most {NameMaximumLength}.");

	private static readonly ResultProblem s_descriptionTooLong =
		new("Description is too long", $"Description length must be at most {DescriptionMaximumLength}.");

	private Room(
		Snowflake id,
		String name,
		String description,
		Boolean isEntrance,
		String concurrencyStamp,
		IReadOnlyList<RoomConnection> connections,
		DateTime creationDate,
		Boolean isRemoved)
	{
		Id = id;
		Name = name;
		Description = description;
		IsEntrance = isEntrance;
		ConcurrencyStamp = concurrencyStamp;
		Connections = connections;
		CreationDate = creationDate;
		IsRemoved = isRemoved;
	}

	internal Snowflake Id { get; }

	internal String Name { get; private set; }

	internal String Description { get; private set; }

	internal Boolean IsEntrance { get; private set; }

	internal String ConcurrencyStamp { get; private set; }

	// Readonly navigation property
	internal IReadOnlyList<RoomConnection> Connections { get; }

	internal DateTime CreationDate { get; }

	internal Boolean IsRemoved { get; private set; }

	internal static Result<Room> Create(Snowflake id, String name, String description, Boolean isEntrance)
	{
		var problems = new Items<ResultProblem>();

		switch (name.Length)
		{
			case < NameMinimumLength: problems.Add(s_nameTooShort); break;
			case > NameMaximumLength: problems.Add(s_nameTooLong); break;
			default: break;
		}

		if (description.Length > DescriptionMaximumLength)
		{
			problems.Add(s_descriptionTooLong);
		}

		if (problems.Count > 0)
		{
			return new ResultProblemValues(problems);
		}

		var room = new Room(id, name, description, isEntrance, GenerateConcurrencyStamp(), [], DateTime.UtcNow, false);
		room.Events.Add(new RoomCreatedEvent(room));
		return room;
	}

	internal Result Modify(
		String? name = null,
		String? description = null,
		Boolean? isEntrance = null)
	{
		var modified = false;
		var problems = new Items<ResultProblem>();

		if (name is not null &&
		    name != Name)
		{
			Name = name;
			modified = true;

			switch (name.Length)
			{
				case < NameMinimumLength: problems.Add(s_nameTooShort); break;
				case > NameMaximumLength: problems.Add(s_nameTooLong); break;
				default: break;
			}
		}

		if (description is not null &&
		    description != Description)
		{
			Description = description;
			modified = true;

			if (description.Length > DescriptionMaximumLength)
			{
				problems.Add(s_descriptionTooLong);
			}
		}

		if (isEntrance is not null &&
		    isEntrance != IsEntrance)
		{
			IsEntrance = (Boolean)isEntrance;
			modified = true;
		}

		if (problems.Count > 0)
		{
			return new ResultProblemValues(problems);
		}

		if (modified && Events.All(e => e is not RoomUpdatedEvent))
		{
			Events.Add(new RoomUpdatedEvent(this));
		}

		return Result.Success;
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
