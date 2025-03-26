using System.Text.Json.Serialization;

namespace Aula.Server.Core.Api;

[JsonSerializable(typeof(UserData))]
internal sealed partial class ApiJsonContext : JsonSerializerContext;
