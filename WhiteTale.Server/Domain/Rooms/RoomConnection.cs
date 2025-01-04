namespace WhiteTale.Server.Domain.Rooms;

internal sealed class RoomConnection
{
	public required UInt64 Id { get; init; }

	public required UInt64 SourceRoomId { get; init; }

	public required UInt64 TargetRoomId { get; init; }
}
