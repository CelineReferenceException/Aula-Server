namespace WhiteTale.Server.Features.Users.SetCurrentRoom;

internal sealed class SetCurrentRoomRequestBody
{
	public required UInt64 RoomId { get; init; }
}
