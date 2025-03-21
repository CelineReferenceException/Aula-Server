using System.Text.Json.Serialization;

namespace Aula.Server.Core.Gateway;

/// <summary>
///     The name of the events that can be dispatched in a gateway session.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<EventType>))]
internal enum EventType
{
	#region Send

	/// <summary>
	///     The gateway connection is ready.
	/// </summary>
	Ready,

	/// <summary>
	///     A new room has been created.
	/// </summary>
	RoomCreated,

	/// <summary>
	///     A room has been updated.
	/// </summary>
	RoomUpdated,

	/// <summary>
	///     A room has been removed.
	/// </summary>
	RoomRemoved,

	/// <summary>
	///     A connection between two rooms has been created.
	/// </summary>
	RoomConnectionCreated,

	/// <summary>
	///     A room connection has been removed.
	/// </summary>
	RoomConnectionRemoved,

	/// <summary>
	///     A user has been updated.
	/// </summary>
	UserUpdated,

	/// <summary>
	///     A user has moved from room.
	/// </summary>
	UserCurrentRoomUpdated,

	/// <summary>
	///     A new message has been sent.
	/// </summary>
	MessageCreated,

	/// <summary>
	///     A message has been deleted.
	/// </summary>
	MessageRemoved,

	/// <summary>
	///     A user has started typing in a room.
	/// </summary>
	UserStartedTyping,

	/// <summary>
	///     A user stopped typing in a room.
	/// </summary>
	UserStoppedTyping,

	/// <summary>
	///     A user has been banned.
	/// </summary>
	BanCreated,

	/// <summary>
	///     A user has been unbanned.
	/// </summary>
	BanRemoved,

	#endregion

	#region Receive

	/// <summary>
	///     Updates the current presence status for the current user.
	/// </summary>
	UpdatePresence,

	#endregion
}
