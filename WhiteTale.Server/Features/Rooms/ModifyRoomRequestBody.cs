namespace WhiteTale.Server.Features.Rooms;

internal sealed class ModifyRoomRequestBody
{
	public String? Name { get; init; }

	public String? Description { get; init; }

	public Boolean? IsEntrance { get; init; }
}
