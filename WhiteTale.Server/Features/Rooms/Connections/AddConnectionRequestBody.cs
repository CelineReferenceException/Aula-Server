namespace WhiteTale.Server.Features.Rooms.Connections;

internal sealed class AddConnectionRequestBody
{
	public required UInt64 TargetId { get; init; }
}
