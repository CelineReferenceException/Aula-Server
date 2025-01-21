namespace WhiteTale.Server.Features.Rooms.Connections;

internal sealed class RemoveConnectionRequestBody
{
	public required UInt64 RoomId { get; init; }
}
