using System.Text.Json.Serialization;

namespace Aula.Server.Core.Api.Users;

[JsonSerializable(typeof(ModifyCurrentUserRequestBody))]
[JsonSerializable(typeof(SetPermissionsRequestBody))]
[JsonSerializable(typeof(SetUserRoomRequestBody))]
[JsonSerializable(typeof(UpdatePresenceEventData))]
[JsonSerializable(typeof(UserCurrentRoomUpdatedEventData))]
internal sealed partial class UserJsonContext : JsonSerializerContext;
