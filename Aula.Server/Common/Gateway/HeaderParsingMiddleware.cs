using System.Buffers;
using System.Buffers.Text;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Aula.Server.Common.Gateway;

internal sealed class HeaderParsingMiddleware
{
	private readonly RequestDelegate _next;

	public HeaderParsingMiddleware(RequestDelegate next)
	{
		_next = next;
	}

	public async Task InvokeAsync(HttpContext httpContext)
	{
		if (!httpContext.WebSockets.IsWebSocketRequest ||
		    !httpContext.Request.Headers.TryGetValue("Sec-WebSocket-Protocol", out var protocols) ||
		    protocols.Count == 0)
		{
			await _next(httpContext);
			return;
		}

		for (var i = 0; i < protocols.Count; i++)
		{
			var protocol = protocols[i];

			if (protocol == null ||
			    !protocol.StartsWith("h_"))
			{
				continue;
			}

			try
			{
				var headersAsBase64Url = protocol.AsSpan()[2..];
				var headersUtf8MaxDecodedLength = Base64Url.GetMaxDecodedLength(headersAsBase64Url.Length);
				var headersUtf8Buffer = ArrayPool<Byte>.Shared.Rent(headersUtf8MaxDecodedLength);
				if (!Base64Url.TryDecodeFromChars(headersAsBase64Url, headersUtf8Buffer, out var bytesWritten))
				{
					break;
				}

				var headersAsJsonString = Encoding.UTF8.GetString(headersUtf8Buffer[..bytesWritten]);
				ArrayPool<Byte>.Shared.Return(headersUtf8Buffer);

				var headers = JsonSerializer.Deserialize<Dictionary<String, String>>(headersAsJsonString)?.AsEnumerable() ?? [];
				foreach (var header in headers)
				{
					httpContext.Request.Headers.Append(header.Key, header.Value);
				}

				var remainingProtocols = protocols.ToArray().ToList();
				remainingProtocols.RemoveAt(i);

				httpContext.Request.Headers.SecWebSocketProtocol = new StringValues([.. remainingProtocols,]);

				if (protocols.Count == 1)
				{
					// To comply with RFC 6455, if no extra protocols were defined return the headers as the selected protocol.
					httpContext.Response.Headers.SecWebSocketProtocol = protocol;
				}

				break;
			}
			catch (JsonException)
			{
				// Prevent stopping if the received JSON was invalid.
				break;
			}
		}

		await _next(httpContext);
	}
}
