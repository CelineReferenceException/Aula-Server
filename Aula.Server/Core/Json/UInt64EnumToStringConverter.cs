using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aula.Server.Core.Json;

[SuppressMessage("Performance", "CA1812: Avoid uninstantiated internal classes")]
internal sealed class UInt64EnumToStringConverter<T> : JsonConverter<T> where T : struct, Enum
{
	public override Boolean CanConvert(Type typeToConvert)
	{
		return typeToConvert.IsEnum && typeToConvert.BaseType == typeof(UInt64);
	}

	public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType is not JsonTokenType.String ||
		    !Enum.TryParse<T>(reader.GetString(), out var result))
		{
			throw new JsonException();
		}

		return result;
	}

	public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
	{
		writer.WriteStringValue(Unsafe.As<T, UInt64>(ref value).ToString());
	}
}
