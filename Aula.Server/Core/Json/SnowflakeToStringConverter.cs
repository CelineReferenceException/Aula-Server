using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Aula.Server.Core.Domain;

namespace Aula.Server.Core.Json;

internal sealed class SnowflakeToStringConverter : JsonConverter<Snowflake>
{
	[SuppressMessage("Style", "IDE0072:Add missing cases", Justification = "Readability.")]
	public override Snowflake Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		return reader.TokenType switch
		{
			JsonTokenType.Number => reader.GetUInt64(),
			JsonTokenType.String => UInt64.Parse(reader.ValueSpan),
			_ => throw new JsonException(),
		};
	}

	public override void Write(Utf8JsonWriter writer, Snowflake value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString());
	}
}
