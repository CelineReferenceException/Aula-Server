namespace WhiteTale.Server.Features.Rooms;

internal sealed class CreateRoomRequestBody
{
	public required String Name { get; init; }

	public String? Description { get; init; }

	public Boolean? IsEntrance { get; init; }
}
