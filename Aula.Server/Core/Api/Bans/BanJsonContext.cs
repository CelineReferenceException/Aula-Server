using System.Text.Json.Serialization;

namespace Aula.Server.Core.Api.Bans;

[JsonSerializable(typeof(BanData))]
[JsonSerializable(typeof(CreateUserBanRequestBody))]
[JsonSerializable(typeof(GetCurrentUserBanStatusResponseBody))]
internal sealed partial class BanJsonContext : JsonSerializerContext;
