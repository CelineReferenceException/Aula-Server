namespace WhiteTale.Server.Features.Rooms;

internal sealed record RoomData
{
	public required UInt64 Id { get; init; }

	public required String Name { get; init; }

	public String? Description { get; init; }

	public required Boolean IsEntrance { get; init; }

	public required DateTime CreationTime { get; init; }
}
