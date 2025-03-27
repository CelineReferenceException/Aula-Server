using System.Text.Json.Serialization;

namespace Aula.Server.Core.Api.Identity;

[JsonSerializable(typeof(LogInRequestBody))]
[JsonSerializable(typeof(LogInResponseBody))]
[JsonSerializable(typeof(RegisterRequestBody))]
[JsonSerializable(typeof(ResetPasswordRequestBody))]
internal sealed partial class IdentityJsonContext : JsonSerializerContext;
