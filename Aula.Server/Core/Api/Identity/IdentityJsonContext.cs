using System.Text.Json.Serialization;

namespace Aula.Server.Core.Api.Identity;

[JsonSerializable(typeof(LogInRequestBody))]
[JsonSerializable(typeof(LogInResponse))]
[JsonSerializable(typeof(RegisterRequestBody))]
[JsonSerializable(typeof(ResetPasswordRequestBody))]
internal sealed partial class IdentityJsonContext : JsonSerializerContext;
