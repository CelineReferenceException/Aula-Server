namespace WhiteTale.Server.Domain.Rooms;

internal sealed class Room
{
	internal const Int32 NameMinimumLength = 3;
	internal const Int32 NameMaximumLength = 32;
	internal const Int32 DescriptionMinimumLength = 1;
	internal const Int32 DescriptionMaximumLength = 2048;

	public required UInt64 Id { get; init; }

	public required String Name { get; set; }

	public String? Description { get; set; }

	public Boolean IsEntrance { get; set; }
}
