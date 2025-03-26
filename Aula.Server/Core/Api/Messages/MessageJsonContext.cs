using System.Text.Json.Serialization;

namespace Aula.Server.Core.Api.Messages;

[JsonSerializable(typeof(MessageData))]
[JsonSerializable(typeof(MessageUserJoinData))]
[JsonSerializable(typeof(MessageUserLeaveData))]
[JsonSerializable(typeof(SendMessageRequestBody))]
internal sealed partial class MessageJsonContext : JsonSerializerContext;
