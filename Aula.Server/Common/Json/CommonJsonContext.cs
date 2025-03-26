using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Aula.Server.Common.Json;

[JsonSerializable(typeof(ProblemDetails))]
[JsonSerializable(typeof(ValidationProblemDetails))]
internal sealed partial class CommonJsonContext : JsonSerializerContext;
