namespace WhiteTale.Server.Domain.Characters;

internal sealed class Character
{
	internal const Int32 DisplayNameMinimumLength = 3;
	internal const Int32 DisplayNameMaximumLength = 32;
	internal const Int32 UserNameMinimumLength = 6;
	internal const Int32 UserNameMaximumLength = 32;
	internal const Int32 DescriptionMinimumLength = 1;
	internal const Int32 DescriptionMaximumLength = 1024;

	public required UInt64 Id { get; init; }

	public required String DisplayName { get; set; }

	public String? Description { get; set; }

	public required CharacterOwnerType OwnerType { get; init; }

	public Permissions Permissions { get; set; }

	public Presence Presence { get; set; }

	public UInt64? CurrentRoomId { get; set; }

	public required DateTime CreationTime { get; init; }

	public required String ConcurrencyStamp { get; set; }
}
