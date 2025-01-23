using System.Text.Json.Serialization;

namespace WhiteTale.Server.Features.Gateway;

/// <summary>
///     The name of the events that can be dispatched in a gateway session.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<EventType>))]
internal enum EventType
{
	#region Send

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

	#endregion

	#region Receive

	/// <summary>
	///     Updates the current presence status for the current user.
	/// </summary>
	UpdatePresence,

	#endregion
}
