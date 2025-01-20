using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WhiteTale.Server.Common.JsonSerialization;

internal sealed class Int64StringConverter : JsonConverter<Int64>
{
	[SuppressMessage("Style", "IDE0072:Add missing cases")]
	public override Int64 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		return reader.TokenType switch
		{
			JsonTokenType.Number => reader.GetInt64(),
			JsonTokenType.String => Int64.Parse(reader.ValueSpan, CultureInfo.InvariantCulture),
			_ => throw new JsonException()
		};
	}

	public override void Write(Utf8JsonWriter writer, Int64 value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(value.ToString(CultureInfo.InvariantCulture));
	}
}
