using System.Text.Json;

namespace WhiteTale.Server.Common.JsonSerialization;

internal static class ObjectExtensions
{
	internal static Byte[] GetJsonUtf8Bytes<TValue>(this TValue value)
	{
		return GetJsonUtf8Bytes(value, JsonSerializerOptions.Web);
	}

	internal static Byte[] GetJsonUtf8Bytes<TValue>(this TValue value, JsonSerializerOptions options)
	{
		return JsonSerializer.SerializeToUtf8Bytes(value, options);
	}
}
