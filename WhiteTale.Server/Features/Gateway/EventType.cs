using System.Text.Json.Serialization;

namespace WhiteTale.Server.Features.Gateway;

[JsonConverter(typeof(JsonStringEnumConverter<EventType>))]
internal enum EventType
{
	RoomCreated,
	RoomUpdated,
	RoomRemoved,
	RoomConnectionCreated,
	RoomConnectionRemoved,
	UserUpdated,
	UserCurrentRoomUpdated,
	MessageCreated,
	MessageRemoved,
}
