namespace Aula.Server.Core.Features.Rooms;

/// <summary>
///     Occurs when a user is typing a message.
/// </summary>
internal sealed record UserTypingEventData
{
	/// <summary>
	///     The ID of the user typing.
	/// </summary>
	public required Snowflake UserId { get; init; }

	/// <summary>
	///     The room where the user is typing.
	/// </summary>
	public required Snowflake RoomId { get; init; }
}
