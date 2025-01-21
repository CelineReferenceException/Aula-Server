namespace WhiteTale.Server.Features.Rooms.Messages;

internal sealed class MessageData
{
	public required UInt64 Id { get; init; }

	public required MessageType Type { get; init; }

	public required MessageFlags Flags { get; init; }

	public required UInt64 AuthorId { get; init; }

	public required MessageTarget Target { get; init; }

	public required UInt64 TargetId { get; init; }

	public String? Content { get; init; }

	public required DateTime CreationTime { get; init; }
}
