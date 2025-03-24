using FluentValidation.Results;

namespace Aula.Server.Domain.Users;

internal sealed class User : DefaultDomainEntity
{
	internal const Int32 DisplayNameMinimumLength = 3;
	internal const Int32 DisplayNameMaximumLength = 32;
	internal const Int32 UserNameMinimumLength = 6;
	internal const Int32 UserNameMaximumLength = 32;
	internal const Int32 DescriptionMaximumLength = 1024;
	internal const Int32 PasswordMaximumLength = 128;

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

	internal static Result<User, ValidationFailure> Create(
		Snowflake id,
		String userName,
		String? email,
		String? displayName,
		String description,
		UserType type,
		Permissions permissions)
	{
		var user = new User(id, userName, email, false, null, GenerateSecurityStamp(), 0, null, displayName ?? userName, description,
			permissions, type, Presence.Offline, null, DateTime.UtcNow, false, GenerateConcurrencyStamp());

		var validationResult = UserValidator.Instance.Validate(user);
		return validationResult.IsValid
			? user
			: new ResultErrorValues<ValidationFailure>(validationResult.Errors);
	}

	internal Result<ValidationFailure> Modify(
		String? displayName = null,
		String? description = null,
		Permissions? permissions = null,
		Presence? presence = null)
	{
		var oldDisplayName = DisplayName;
		var oldDescription = Description;
		var oldPermissions = Permissions;
		var oldPresence = Presence;
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
			return Result<ValidationFailure>.Success;
		}

		var validationResult = UserValidator.Instance.Validate(this);
		if (!validationResult.IsValid)
		{
			DisplayName = oldDisplayName;
			Description = oldDescription;
			Permissions = oldPermissions;
			Presence = oldPresence;

			return new ResultErrorValues<ValidationFailure>(validationResult.Errors);
		}

		if (Events.All(e => e is not UserUpdatedEvent))
		{
			Events.Add(new UserUpdatedEvent(this));
		}

		return Result<ValidationFailure>.Success;
	}

	internal void SetCurrentRoom(Snowflake? currentRoomId)
	{
		if (CurrentRoomId == currentRoomId)
		{
			return;
		}

		var previousCurrentRoomId = CurrentRoomId;
		CurrentRoomId = currentRoomId;

		Events.Add(new UserCurrentRoomUpdatedEvent(Id, previousCurrentRoomId, CurrentRoomId));
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
		Events.Add(new UserSecurityStampUpdatedEvent(this));
	}

	internal void Remove()
	{
		if (IsRemoved)
		{
			return;
		}

		IsRemoved = true;
		SecurityStamp = null;
		Email = null;

		var nameStamp = Guid.CreateVersion7().ToString("N");
		UserName = $"removed_user_{nameStamp}";
		DisplayName = $"Removed user {nameStamp}";

		Events.Add(new UserRemovedEvent(this));
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
