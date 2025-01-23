namespace WhiteTale.Server.Features.Users.CurrentRoom;

internal sealed class SetCurrentRoomRequestBody
{
	public required UInt64 RoomId { get; init; }
}
