﻿using System.Text.Json;

namespace Aula.Server.Core.Json;

internal static class ObjectExtensions
{
	internal static Byte[] GetJsonUtf8Bytes<TValue>(this TValue value, JsonSerializerOptions options)
	{
		return JsonSerializer.SerializeToUtf8Bytes(value, options);
	}
}
