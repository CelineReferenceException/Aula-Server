#pragma warning disable CS8618
namespace WhiteTale.Server.Domain.Characters;

internal sealed class Character : DefaultDomainEntity
{
	internal const Int32 DisplayNameMinimumLength = 3;
	internal const Int32 DisplayNameMaximumLength = 32;
	internal const Int32 UserNameMinimumLength = 6;
	internal const Int32 UserNameMaximumLength = 32;
	internal const Int32 DescriptionMinimumLength = 1;
	internal const Int32 DescriptionMaximumLength = 1024;

	internal UInt64 Id { get; private init; }

	internal String DisplayName { get; private set; }

	internal String? Description { get; private set; }

	internal CharacterOwnerType OwnerType { get; private init; }

	internal Presence Presence { get; private set; }

	internal UInt64? CurrentRoomId { get; private set; }

	internal DateTime CreationTime { get; private init; }

	internal String ConcurrencyStamp { get; private set; }

	internal static Character Create(UInt64 id, String displayName, CharacterOwnerType ownerType)
	{
		var character = new Character
		{
			Id = id,
			DisplayName = displayName,
			OwnerType = ownerType,
			CreationTime = DateTime.UtcNow,
			ConcurrencyStamp = Guid.NewGuid().ToString("N")
		};

		return character;
	}

	internal void Modify(String? displayName = null, String? description = null)
	{
		var modified = false;

		if (displayName is not null &&
		    displayName != DisplayName)
		{
			DisplayName = displayName;
			modified = true;
		}

		if (description is not null &&
		    description != Description)
		{
			Description = description;
			modified = true;
		}

		if (!modified)
		{
			return;
		}

		ConcurrencyStamp = Guid.NewGuid().ToString("N");
		AddEvent(new CharacterUpdatedEvent(this));
	}

	internal void SetPresence(Presence presence)
	{
		if (Presence == presence)
		{
			return;
		}

		ConcurrencyStamp = Guid.NewGuid().ToString("N");
		Presence = presence;

		AddEvent(new CharacterPresenceUpdatedEvent(Id, Presence));
	}

	internal void SetCurrentRoom(UInt64? currentRoomId)
	{
		if (CurrentRoomId == currentRoomId)
		{
			return;
		}

		ConcurrencyStamp = Guid.NewGuid().ToString("N");
		CurrentRoomId = currentRoomId;

		AddEvent(new CharacterCurrentRoomUpdatedEvent(Id, CurrentRoomId));
	}
}
