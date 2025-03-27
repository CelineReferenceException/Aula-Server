using System.Text.Json.Serialization;

namespace Aula.Server.Core.Api.Bans;

[JsonSerializable(typeof(BanData))]
[JsonSerializable(typeof(CreateBanRequestBody))]
[JsonSerializable(typeof(GetCurrentUserBanStatusResponseBody))]
internal sealed partial class BanJsonContext : JsonSerializerContext;
