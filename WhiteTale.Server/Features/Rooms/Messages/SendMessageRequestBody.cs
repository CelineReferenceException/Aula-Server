namespace WhiteTale.Server.Features.Rooms.Messages;

/// <summary>
///     Holds the data required to send a message.
/// </summary>
internal sealed class SendMessageRequestBody
{
	/// <summary>
	///     The type of the message.
	/// </summary>
	public required MessageType Type { get; init; }

	/// <summary>
	///     The flags of the message.
	/// </summary>
	public MessageFlags? Flags { get; init; }

	/// <summary>
	///     Who or what the message targets.
	/// </summary>
	public MessageTarget? Target { get; init; }

	/// <summary>
	///     The text content of the message. Required for <see cref="MessageType.Standard" /> messages.
	/// </summary>
	public String? Content { get; init; }
}
