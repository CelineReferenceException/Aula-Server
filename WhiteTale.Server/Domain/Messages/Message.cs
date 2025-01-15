namespace WhiteTale.Server.Domain.Messages;

internal sealed class Message
{
	internal const Int32 ContentMaximumLength = 2048;

	public required UInt64 Id { get; init; }

	public required MessageFlags Flags { get; init; }

	public required UInt64 AuthorId { get; init; }

	public required MessageTarget Target { get; init; }

	public UInt64? RoomId { get; init; }

	public required String Content { get; init; }

	public required DateTime CreationTime { get; init; }
}
