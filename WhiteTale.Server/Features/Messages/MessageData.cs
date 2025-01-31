namespace WhiteTale.Server.Features.Messages;

/// <summary>
///     Represents a message within a room.
/// </summary>
internal sealed record MessageData
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
	///     The type of author who sent the message.
	/// </summary>
	public required MessageAuthor AuthorType { get; init; }

	/// <summary>
	///     The ID of the author who created the message.
	/// </summary>
	public UInt64? AuthorId { get; init; }

	/// <summary>
	///     Who or what the message targets.
	/// </summary>
	public required MessageTarget TargetType { get; init; }

	/// <summary>
	///     The ID of the message's main target.
	/// </summary>
	public required UInt64 TargetId { get; init; }

	/// <summary>
	///     The text content of the message.
	/// </summary>
	public String? Content { get; init; }

	/// <summary>
	///     The room join data associated with the message. Only present for <see cref="MessageType.UserJoin" />
	/// </summary>
	public MessageUserJoinData? JoinData { get; init; }

	/// <summary>
	///     The room leave data associated with the message. Only present for <see cref="MessageType.UserLeave" />
	/// </summary>
	public MessageUserLeaveData? LeaveData { get; init; }

	/// <summary>
	///     The date and time when the message was created.
	/// </summary>
	public required DateTime CreationTime { get; init; }
}
