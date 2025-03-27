using System.Text.Json.Serialization;

namespace Aula.Server.Core.Api.Bots;

[JsonSerializable(typeof(CreateBotRequestBody))]
[JsonSerializable(typeof(CreateBotResponseBody))]
[JsonSerializable(typeof(ResetBotTokenResponseBody))]
internal sealed partial class BotJsonContext : JsonSerializerContext;
