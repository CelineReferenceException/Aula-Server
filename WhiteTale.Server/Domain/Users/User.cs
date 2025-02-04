using FluentValidation;

#pragma warning disable CS8618
namespace WhiteTale.Server.Domain.Users;

internal sealed class User : DefaultDomainEntity
{
	internal const Int32 DisplayNameMinimumLength = 3;
	internal const Int32 DisplayNameMaximumLength = 32;
	internal const Int32 UserNameMinimumLength = 6;
	internal const Int32 UserNameMaximumLength = 32;
	internal const Int32 DescriptionMinimumLength = 1;
	internal const Int32 DescriptionMaximumLength = 1024;
	private static readonly UserValidator s_validator = new();

	private User()
	{
	}

	internal UInt64 Id { get; private init; }

	internal String UserName { get; private init; }

	internal String Email { get; private init; }

	internal Boolean EmailConfirmed { get; private set; }

	internal String Password { get; private set; }

	internal String SecurityStamp { get; private set; }

	internal Int32 AccessFailedCount { get; private set; }

	internal String DisplayName { get; private set; }

	internal String? Description { get; private set; }

	internal Permissions Permissions { get; private set; }

	internal UserOwnerType OwnerType { get; private init; }

	internal Presence Presence { get; set; }

	internal UInt64? CurrentRoomId { get; private set; }

	internal DateTime CreationTime { get; private init; }

	internal String ConcurrencyStamp { get; private set; }

	internal static User Create(
		UInt64 id,
		String userName,
		String email,
		String password,
		String? displayName,
		UserOwnerType ownerType,
		Permissions permissions)
	{
		var user = new User
		{
			Id = id,
			UserName = userName,
			Email = email,
			Password = password,
			DisplayName = displayName ?? userName,
			Permissions = permissions,
			OwnerType = ownerType,
			CreationTime = DateTime.UtcNow,
			ConcurrencyStamp = GenerateStamp(),
			SecurityStamp = GenerateSecurityStamp(),
		};

		s_validator.ValidateAndThrow(user);

		return user;
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

		s_validator.ValidateAndThrow(this);

		AddEvent(new UserUpdatedEvent(this));
	}

	internal void SetCurrentRoom(UInt64? currentRoomId)
	{
		if (CurrentRoomId == currentRoomId)
		{
			return;
		}

		var previousCurrentRoomId = CurrentRoomId;
		CurrentRoomId = currentRoomId;

		s_validator.ValidateAndThrow(this);

		AddEvent(new UserCurrentRoomUpdatedEvent(Id, previousCurrentRoomId, CurrentRoomId));
	}

	internal void UpdateConcurrencyStamp()
	{
		ConcurrencyStamp = GenerateStamp();
	}

	internal void UpdateSecurityStamp()
	{
		SecurityStamp = GenerateSecurityStamp();
		AddEvent(new UserSecurityStampUpdatedEvent(this));
	}

	private static String GenerateStamp()
	{
		return Guid.CreateVersion7().ToString("N");
	}

	private static String GenerateSecurityStamp()
	{
		return Guid.CreateVersion7().ToString("N");
	}
}
