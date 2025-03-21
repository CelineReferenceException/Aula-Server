using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aula.Server.Core.Json;

internal sealed class Int64StringConverter : JsonConverter<Int64>
{
	[SuppressMessage("Style", "IDE0072:Add missing cases")]
	public override Int64 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		return reader.TokenType switch
		{
			JsonTokenType.Number => reader.GetInt64(),
			JsonTokenType.String => Int64.Parse(reader.ValueSpan),
			_ => throw new JsonException(),
		};
	}

	public override void Write(Utf8JsonWriter writer, Int64 value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString());
	}
}
