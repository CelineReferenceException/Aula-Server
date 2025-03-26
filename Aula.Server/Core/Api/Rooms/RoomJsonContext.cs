using System.Text.Json.Serialization;

namespace Aula.Server.Core.Api.Rooms;

[JsonSerializable(typeof(CreateRoomRequestBody))]
[JsonSerializable(typeof(ModifyRoomRequestBody))]
[JsonSerializable(typeof(RoomConnectionEventData))]
[JsonSerializable(typeof(RoomData))]
[JsonSerializable(typeof(SetRoomConnectionsRequestBody))]
[JsonSerializable(typeof(UserTypingEventData))]
internal sealed partial class RoomJsonContext : JsonSerializerContext;
