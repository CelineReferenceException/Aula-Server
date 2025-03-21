namespace Aula.Server.Domain.Users;

internal sealed class User : DefaultDomainEntity
{
	internal const Int32 DisplayNameMinimumLength = 3;
	internal const Int32 DisplayNameMaximumLength = 32;
	internal const Int32 UserNameMinimumLength = 6;
	internal const Int32 UserNameMaximumLength = 32;
	internal const Int32 DescriptionMaximumLength = 1024;
	internal const Int32 PasswordMaximumLength = 128;

	internal Snowflake Id { get; }

	internal String UserName { get; private set; }

	internal String? Email { get; private set; }

	internal Boolean EmailConfirmed { get; private set; }

	internal String? PasswordHash { get; set; }

	internal String? SecurityStamp { get; private set; }

	internal Int32 AccessFailedCount { get; private set; }

	internal DateTime? LockoutEndTime { get; private set; }

	internal String DisplayName { get; private set; }

	internal String Description { get; private set; }

	internal Permissions Permissions { get; private set; }

	internal UserType Type { get; }

	internal Presence Presence { get; set; }

	internal Snowflake? CurrentRoomId { get; private set; }

	internal DateTime CreationDate { get; }

	internal Boolean IsRemoved { get; private set; }

	internal String ConcurrencyStamp { get; private set; }

	internal User(
		Snowflake id,
		String userName,
		String? email,
		String? displayName,
		String description,
		UserType type,
		Permissions permissions)
	{
		switch (userName.Length)
		{
			case < UserNameMinimumLength:
				throw new ArgumentOutOfRangeException(nameof(userName),
					$"{nameof(userName)} length must be at least {UserNameMinimumLength}.");
			case > UserNameMaximumLength:
				throw new ArgumentOutOfRangeException(nameof(userName),
					$"{nameof(userName)} length must be at most ${UserNameMaximumLength}.");
			default: break;
		}

		switch (type)
		{
			case UserType.Standard when email is null:
				throw new ArgumentNullException(nameof(email),
					$"${nameof(email)} cannot be null when {nameof(type)} is {UserType.Standard}.");
			case UserType.Bot when email is not null:
				throw new ArgumentNullException(nameof(email), $"{nameof(email)} should be null when {nameof(type)} is {UserType.Bot}.");
			case UserType.Standard or UserType.Bot:
			default: break;
		}

		if (displayName is not null)
		{
			switch (displayName.Length)
			{
				case < DisplayNameMinimumLength:
					throw new ArgumentOutOfRangeException(nameof(displayName),
						$"{nameof(displayName)} length must be at least {DisplayNameMinimumLength}.");
				case > DisplayNameMaximumLength:
					throw new ArgumentOutOfRangeException(nameof(displayName),
						$"{nameof(displayName)} length must be at most ${DisplayNameMaximumLength}.");
				default: break;
			}
		}

		if (description.Length > DescriptionMaximumLength)
		{
			throw new ArgumentOutOfRangeException(nameof(description),
				$"{nameof(description)} length must be at most ${DescriptionMaximumLength}.");
		}

		if (!Enum.IsDefined(type))
		{
			throw new ArgumentOutOfRangeException(nameof(type));
		}

		if (!permissions.IsEnumFlagDefined())
		{
			throw new ArgumentOutOfRangeException(nameof(permissions));
		}

		Id = id;
		UserName = userName.ToUpper();
		Email = email?.ToUpper();
		DisplayName = displayName ?? userName;
		Description = description ?? "";
		Permissions = permissions;
		Type = type;
		CreationDate = DateTime.UtcNow;
		ConcurrencyStamp = GenerateConcurrencyStamp();
		SecurityStamp = GenerateSecurityStamp();
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

		if (description is not null &&
		    description != Description)
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

		AddEvent(new UserCurrentRoomUpdatedEvent(Id, previousCurrentRoomId, CurrentRoomId));
	}

	internal void UpdateConcurrencyStamp()
	{
		ConcurrencyStamp = GenerateConcurrencyStamp();
	}

	internal void IncrementAccessFailedCount()
	{
		AccessFailedCount++;
	}

	internal void ResetAccessFailedCount()
	{
		AccessFailedCount = 0;
	}

	internal void Lockout(TimeSpan time)
	{
		LockoutEndTime = DateTime.UtcNow.Add(time);
	}

	internal void RemoveLockout()
	{
		LockoutEndTime = null;
	}

	internal void ConfirmEmail()
	{
		EmailConfirmed = true;
	}

	internal void ChangePassword(String newPasswordHash)
	{
		PasswordHash = newPasswordHash;
	}

	internal void UpdateSecurityStamp()
	{
		SecurityStamp = GenerateSecurityStamp();
		AddEvent(new UserSecurityStampUpdatedEvent(this));
	}

	internal void Remove()
	{
		IsRemoved = true;
		SecurityStamp = null;
		Email = null;

		var nameStamp = Guid.CreateVersion7().ToString("N");
		UserName = $"removed_user_{nameStamp}";
		DisplayName = $"Removed user {nameStamp}";

		AddEvent(new UserRemovedEvent(this));
	}

	private static String GenerateConcurrencyStamp()
	{
		return Guid.CreateVersion7().ToString("N");
	}

	private static String GenerateSecurityStamp()
	{
		return Guid.CreateVersion7().ToString("N");
	}
}
