using System.Text.Json.Serialization;

namespace Aula.Server.Core.Api.Bots;

[JsonSerializable(typeof(CreateBotRequestBody))]
[JsonSerializable(typeof(CreateBotResponse))]
[JsonSerializable(typeof(ResetBotTokenResponse))]
internal sealed partial class BotJsonContext : JsonSerializerContext;
