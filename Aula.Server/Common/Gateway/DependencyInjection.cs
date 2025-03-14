using System.Buffers;
using System.Buffers.Text;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.Extensions.Primitives;

namespace Aula.Server.Common.Gateway;

internal static class DependencyInjection
{
	internal static IServiceCollection AddGateway(this IServiceCollection services)
	{
		_ = services.AddOptions<GatewayOptions>()
			.BindConfiguration(GatewayOptions.SectionName)
			.ValidateDataAnnotations()
			.ValidateOnStart();

		_ = services.AddSingleton<GatewayService>();
		_ = services.AddHostedService<RemoveExpiredSessionsService>();

		_ = services.AddWebSockets(options =>
		{
			options.KeepAliveInterval = TimeSpan.Zero;
			options.KeepAliveTimeout = TimeSpan.FromSeconds(60);
		});

		return services;
	}

	internal static TBuilder ExtractHeadersFromWebSocketProtocols<TBuilder>(this TBuilder builder) where TBuilder : IApplicationBuilder
	{
		_ = builder.Use((httpContext, next) =>
		{
			if (!httpContext.Request.Headers.TryGetValue("Sec-WebSocket-Protocol", out var protocols) ||
			    protocols.Count == 0)
			{
				return next(httpContext);
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
					var headersAsBase64 = protocol.AsSpan()[2..];
					var maxDecodedByteLength = Base64.GetMaxDecodedFromUtf8Length(headersAsBase64.Length * 2);
					var decodingBuffer = ArrayPool<Byte>.Shared.Rent(maxDecodedByteLength);
					if (!Convert.TryFromBase64Chars(headersAsBase64, decodingBuffer, out var bytesWritten))
					{
						break;
					}

					var headersAsJsonString = Encoding.UTF8.GetString(decodingBuffer[..bytesWritten]);
					ArrayPool<Byte>.Shared.Return(decodingBuffer);

					var headers = JsonSerializer.Deserialize<Dictionary<String, String>>(headersAsJsonString)?.AsEnumerable() ?? [];
					foreach (var header in headers)
					{
						httpContext.Request.Headers.Append(header.Key, header.Value);
					}

					var remainingProtocols = protocols.ToArray().ToList();
					remainingProtocols.RemoveAt(i);

					httpContext.Request.Headers.SecWebSocketProtocol = new StringValues([.. remainingProtocols,]);
					break;
				}
				catch (JsonException)
				{
					// Prevent stopping if the received JSON was invalid.
				}
			}

			return next(httpContext);
		});

		return builder;
	}
}
