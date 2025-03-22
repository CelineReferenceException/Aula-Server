namespace Aula.Server.Domain.Users;

internal sealed class User : DefaultDomainEntity
{
	internal const Int32 DisplayNameMinimumLength = 3;
	internal const Int32 DisplayNameMaximumLength = 32;
	internal const Int32 UserNameMinimumLength = 6;
	internal const Int32 UserNameMaximumLength = 32;
	internal const Int32 DescriptionMaximumLength = 1024;
	internal const Int32 PasswordMaximumLength = 128;

	private static readonly ResultProblem s_userNameTooShort =
		new("Username is too short", $"Username length must be at least {UserNameMinimumLength}.");

	private static readonly ResultProblem s_userNameTooLong =
		new("Username is too long", $"Username length must be at most {UserNameMaximumLength}.");

	private static readonly ResultProblem s_invalidEmail =
		new("Invalid email", "email is not a valid email address.");

	private static readonly ResultProblem s_standardUserWithNullEmail =
		new("Invalid email", $"Standard users cannot have a null email address.");

	private static readonly ResultProblem s_botUserWithEmail =
		new("Invalid email", $"Bot users cannot have an email address.");

	private static readonly ResultProblem s_displayNameTooShort =
		new("Display name is too short", $"Display name length must be at least {DisplayNameMinimumLength}.");

	private static readonly ResultProblem s_displayNameTooLong =
		new("Display name is too long", $"Display name length must be at most {DisplayNameMaximumLength}.");

	private static readonly ResultProblem s_descriptionTooLong =
		new("Description is too long", $"Display name length must be at most {DisplayNameMaximumLength}.");

	private static readonly ResultProblem s_unknownUserType =
		new("Unknown user type", $"An unknown user type was provided.");

	private static readonly ResultProblem s_unknownPermissions =
		new("Unknown permissions", $"One or more of the permissions provided are not valid.");

	private User(
		Snowflake id,
		String userName,
		String? email,
		Boolean emailConfirmed,
		String? passwordHash,
		String? securityStamp,
		Int32 accessFailedCount,
		DateTime? lockoutEndTime,
		String displayName,
		String description,
		Permissions permissions,
		UserType type,
		Presence presence,
		Snowflake? currentRoomId,
		DateTime creationDate,
		Boolean isRemoved,
		String concurrencyStamp)
	{
		Id = id;
		UserName = userName;
		Email = email;
		EmailConfirmed = emailConfirmed;
		PasswordHash = passwordHash;
		SecurityStamp = securityStamp;
		AccessFailedCount = accessFailedCount;
		LockoutEndTime = lockoutEndTime;
		DisplayName = displayName;
		Description = description;
		Permissions = permissions;
		Type = type;
		Presence = presence;
		CurrentRoomId = currentRoomId;
		CreationDate = creationDate;
		IsRemoved = isRemoved;
		ConcurrencyStamp = concurrencyStamp;
	}

	internal Snowflake Id { get; }

	internal String UserName { get; private set; }

	internal String? Email { get; private set; }

	internal Boolean EmailConfirmed { get; private set; }

	internal String? PasswordHash { get; private set; }

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

	internal static Result<User> Create(
		Snowflake id,
		String userName,
		String? email,
		String? displayName,
		String description,
		UserType type,
		Permissions permissions)
	{
		var errors = new Items<ResultProblem>();

		switch (userName.Length)
		{
			case < UserNameMinimumLength: errors.Add(s_userNameTooShort); break;
			case > UserNameMaximumLength: errors.Add(s_userNameTooLong); break;
			default: break;
		}

		if (email is not null &&
		    !EmailAddressValidator.IsValid(email))
		{
			errors.Add(s_invalidEmail);
		}

		switch (type)
		{
			case UserType.Standard when email is null: errors.Add(s_standardUserWithNullEmail); break;
			case UserType.Bot when email is not null: errors.Add(s_botUserWithEmail); break;
			case UserType.Standard or UserType.Bot:
			default: break;
		}

		if (displayName is not null)
		{
			switch (displayName.Length)
			{
				case < DisplayNameMinimumLength: errors.Add(s_displayNameTooShort); break;
				case > DisplayNameMaximumLength: errors.Add(s_displayNameTooLong); break;
				default: break;
			}
		}

		if (description.Length > DescriptionMaximumLength)
		{
			errors.Add(s_descriptionTooLong);
		}

		if (!Enum.IsDefined(type))
		{
			errors.Add(s_unknownUserType);
		}

		if (!permissions.IsEnumFlagDefined())
		{
			errors.Add(s_unknownPermissions);
		}

		return errors.Count > 0
			? new ResultProblemValues(errors)
			: new User(id, userName, email, false, null, GenerateSecurityStamp(), 0, null, displayName ?? userName, description,
				permissions, type, Presence.Offline, null, DateTime.UtcNow, false, GenerateConcurrencyStamp());
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

	internal void SetCurrentRoom(Snowflake? currentRoomId)
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
		UpdateConcurrencyStamp();
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
