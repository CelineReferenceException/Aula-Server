namespace WhiteTale.Server.Features.Rooms.Connections;

internal sealed class RemoveConnectionRequestBody
{
	public required UInt64 TargetId { get; init; }
}
