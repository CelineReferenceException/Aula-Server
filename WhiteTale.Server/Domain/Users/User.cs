using System.Diagnostics;
using Microsoft.AspNetCore.Identity;

#pragma warning disable CS8618
namespace WhiteTale.Server.Domain.Users;

internal sealed class User : IdentityUser<UInt64>, IDomainEntity
{
	internal const Int32 DisplayNameMinimumLength = 3;
	internal const Int32 DisplayNameMaximumLength = 32;
	internal const Int32 UserNameMinimumLength = 6;
	internal const Int32 UserNameMaximumLength = 32;
	internal const Int32 DescriptionMinimumLength = 1;
	internal const Int32 DescriptionMaximumLength = 1024;
	private readonly List<DomainEvent> _events = [];

	private User()
	{
	}

	internal new UInt64 Id
	{
		get => base.Id;
		private init => base.Id = value;
	}

	internal new String UserName
	{
		get => base.UserName ?? throw new UnreachableException();
		private set => base.UserName = value;
	}

	internal String DisplayName { get; private set; }

	internal String? Description { get; private set; }

	internal Permissions Permissions { get; private set; }

	internal UserOwnerType OwnerType { get; private init; }

	internal Presence Presence { get; set; }

	internal UInt64? CurrentRoomId { get; private set; }

	internal DateTime CreationTime { get; private init; }

	private new String ConcurrencyStamp
	{
		get => base.ConcurrencyStamp ?? throw new UnreachableException();
		set => base.ConcurrencyStamp = value;
	}

	IReadOnlyList<DomainEvent> IDomainEntity.Events => _events;

	internal static User Create(
		UInt64 id,
		String email,
		String userName,
		String? displayName,
		UserOwnerType ownerType,
		Permissions permissions)
	{
		var character = new User
		{
			Id = id,
			Email = email,
			UserName = userName,
			DisplayName = displayName ?? userName,
			Permissions = permissions,
			OwnerType = ownerType,
			CreationTime = DateTime.UtcNow,
			ConcurrencyStamp = Guid.NewGuid().ToString("N"),
		};

		return character;
	}

	internal void Modify(
		String? displayName = null,
		String? description = null,
		Permissions? permissions = null,
		Presence? presence = null)
	{
		var modified = false;

		if (displayName is not null &&
		    displayName != DisplayName)
		{
			DisplayName = displayName;
			modified = true;
		}

		if (description != Description)
		{
			Description = description;
			modified = true;
		}

		if (permissions is not null &&
		    Permissions != permissions)
		{
			Permissions = permissions.Value;
			modified = true;
		}

		if (presence is not null &&
		    Presence != presence)
		{
			Presence = presence.Value;
			modified = true;
		}

		if (!modified)
		{
			return;
		}

		_events.Add(new UserUpdatedEvent(this));
	}

	internal void SetCurrentRoom(UInt64? currentRoomId)
	{
		if (CurrentRoomId == currentRoomId)
		{
			return;
		}

		var previousCurrentRoomId = CurrentRoomId;
		CurrentRoomId = currentRoomId;

		_events.Add(new UserCurrentRoomUpdatedEvent(Id, previousCurrentRoomId, CurrentRoomId));
	}

	internal void UpdateConcurrencyStamp()
	{
		ConcurrencyStamp = Guid.NewGuid().ToString("N");
	}
}
