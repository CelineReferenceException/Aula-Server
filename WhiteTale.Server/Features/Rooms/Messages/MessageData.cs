namespace WhiteTale.Server.Features.Rooms.Messages;

/// <summary>
///     Represents a message within a room.
/// </summary>
internal sealed class MessageData
{
	/// <summary>
	///     The unique identifier of the message.
	/// </summary>
	public required UInt64 Id { get; init; }

	/// <summary>
	///     The type of the message, defines the message's data.
	/// </summary>
	public required MessageType Type { get; init; }

	/// <summary>
	///     The flags of the message, indicates trivial configurations clients should take into account.
	/// </summary>
	public required MessageFlags Flags { get; init; }

	/// <summary>
	///     The ID of the user who created the message.
	/// </summary>
	public required UInt64 AuthorId { get; init; }

	/// <summary>
	///     Who or what the message targets.
	/// </summary>
	public required MessageTarget Target { get; init; }

	/// <summary>
	///     The ID of the message's main target.
	/// </summary>
	public required UInt64 TargetId { get; init; }

	/// <summary>
	///     The text content of the message.
	/// </summary>
	public String? Content { get; init; }

	/// <summary>
	///     The date and time when the message was created.
	/// </summary>
	public required DateTime CreationTime { get; init; }
}
