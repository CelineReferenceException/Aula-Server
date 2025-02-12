namespace Aula.Server.Features.Messages.Gateway;

/// <summary>
///     Occurs when a user is typing a message.
/// </summary>
internal sealed record UserTypingEventData
{
	/// <summary>
	///     The ID of the user typing.
	/// </summary>
	public required UInt64 UserId { get; init; }

	/// <summary>
	///     The room where the user is typing.
	/// </summary>
	public required UInt64 RoomId { get; init; }
}
