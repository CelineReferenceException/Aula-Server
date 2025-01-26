namespace WhiteTale.Server.Features.Users;

internal sealed class SetCurrentRoomRequestBody
{
	public required UInt64 RoomId { get; init; }
}
