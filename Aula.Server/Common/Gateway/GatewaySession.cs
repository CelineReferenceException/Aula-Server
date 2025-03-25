using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using Aula.Server.Common.Json;
using Aula.Server.Core.Api.Users;
using Aula.Server.Domain;
using MediatR;

namespace Aula.Server.Common.Gateway;

internal sealed class GatewaySession
{
	private const Int32 MaxMessageSize = 1024 * 4;
	private readonly Channel<Byte[]> _eventsQueue = Channel.CreateUnbounded<Byte[]>();
	private readonly JsonSerializerOptions _jsonSerializerOptions;
	private readonly IPublisher _publisher;
	private Boolean _hasConnectedBefore;
	private Byte[]? _nextEvent;
	private Boolean _running;
	private WebSocket? _webSocket;

	internal GatewaySession(
		Snowflake userId,
		Intents intents,
		JsonSerializerOptions jsonSerializerOptions,
		IPublisher publisher)
	{
		UserId = userId;
		Intents = intents;
		_jsonSerializerOptions = jsonSerializerOptions;
		_publisher = publisher;
		Id = Guid.NewGuid().ToString("N");
	}

	[MemberNotNullWhen(false, nameof(CloseDate))]
	internal Boolean IsActive => _running;

	internal String Id { get; }

	internal DateTime? CloseDate { get; private set; }

	internal Snowflake UserId { get; }

	internal Intents Intents { get; }

	internal async Task RunAsync(PresenceOptions presence)
	{
		ThrowIfNullWebSocket();

		if (Interlocked.Exchange(ref _running, true))
		{
			throw new InvalidOperationException("Gateway is already running.");
		}

		if (_webSocket.State is not WebSocketState.Open)
		{
			throw new InvalidOperationException("WebSocket is not open.");
		}

		CloseDate = null;

		var receivingTask = RunPayloadReceivingAsync();
		var sendingTask = RunPayloadSendingAsync();

		await _publisher.Publish(new GatewayConnectedEvent
		{
			Session = this,
			Presence = presence,
		});

		if (!_hasConnectedBefore)
		{
			await _publisher.Publish(new GatewaySessionReadyEvent
			{
				Session = this,
			});

			_hasConnectedBefore = true;
		}

		await Task.WhenAll(receivingTask, sendingTask);

		await _publisher.Publish(new GatewayDisconnectedEvent
		{
			Session = this,
		});

		CloseDate = DateTime.UtcNow;
		_running = false;
	}

	internal async Task StopAsync(WebSocketCloseStatus closeStatus)
	{
		ThrowIfNullWebSocket();

		if (!_running)
		{
			return;
		}

		if (_webSocket.State is WebSocketState.Open or WebSocketState.CloseReceived)
		{
			await _webSocket.CloseOutputAsync(closeStatus, String.Empty, CancellationToken.None);
			_webSocket.Dispose();
		}
	}

	internal async Task QueueEventAsync<T>(GatewayPayload<T> payload, CancellationToken cancellationToken = default)
	{
		_ = await _eventsQueue.Writer.WaitToWriteAsync(cancellationToken);
		await _eventsQueue.Writer.WriteAsync(payload.GetJsonUtf8Bytes(_jsonSerializerOptions), cancellationToken);
	}

	internal void SetWebSocket(WebSocket webSocket)
	{
		ArgumentNullException.ThrowIfNull(webSocket, nameof(webSocket));
		if (_running)
		{
			throw new InvalidOperationException("Cannot set the websocket while the gateway is running.");
		}

		_webSocket = webSocket;
	}

	private async Task RunPayloadSendingAsync()
	{
		ThrowIfWebSocketNotOpen();

		while (_webSocket.State is WebSocketState.Open)
		{
			await SendPayloadAsync();
		}
	}

	private async Task SendPayloadAsync()
	{
		ThrowIfWebSocketNotOpen();

		if (_nextEvent is null)
		{
			if (!await _eventsQueue.Reader.WaitToReadAsync())
			{
				return;
			}

			_nextEvent = await _eventsQueue.Reader.ReadAsync();
		}

		try
		{
			await _webSocket.SendAsync(_nextEvent.AsMemory(), WebSocketMessageType.Text, true, CancellationToken.None);
			_nextEvent = null;
		}
		catch (WebSocketException e)
		{
			if (e.WebSocketErrorCode is not WebSocketError.ConnectionClosedPrematurely)
			{
				throw;
			}
		}
	}

	private async Task RunPayloadReceivingAsync()
	{
		ThrowIfWebSocketNotOpen();

		while (_webSocket.State is WebSocketState.Open)
		{
			var message = await ReceiveMessageAsync();
			if (!message.ReceivedSuccessfully)
			{
				await StopAsync((WebSocketCloseStatus)message.CloseStatus);
				continue;
			}

			if (message.Type is WebSocketMessageType.Close)
			{
				await StopAsync(WebSocketCloseStatus.NormalClosure);
				continue;
			}

			try
			{
				var messageText = Encoding.UTF8.GetString(message.Content);
				var payload = JsonSerializer.Deserialize<GatewayPayload>(messageText, _jsonSerializerOptions);
				if (payload is null)
				{
					await StopAsync(WebSocketCloseStatus.InvalidPayloadData);
					break;
				}

				await _publisher.Publish(new PayloadReceivedEvent
				{
					Session = this,
					Payload = payload,
				});
			}
			catch (Exception e) when (e is ArgumentException or JsonException)
			{
				await StopAsync(WebSocketCloseStatus.InvalidPayloadData);
				break;
			}
		}
	}

	private async Task<GatewayReceivedMessage> ReceiveMessageAsync()
	{
		ThrowIfWebSocketNotOpen();

		var buffer = new Byte[1024].AsMemory();
		using var messageBytes = new MemoryStream();

		ValueWebSocketReceiveResult received;
		do
		{
			try
			{
				received = await _webSocket.ReceiveAsync(buffer, CancellationToken.None);
			}
			catch (WebSocketException e)
			{
				if (e.WebSocketErrorCode is not WebSocketError.ConnectionClosedPrematurely)
				{
					throw;
				}

				return GatewayReceivedMessage.Close;
			}

			if (messageBytes.Length + received.Count > MaxMessageSize)
			{
				return GatewayReceivedMessage.MessageTooBig;
			}

			if (received.MessageType is WebSocketMessageType.Binary)
			{
				return GatewayReceivedMessage.InvalidMessageType;
			}

			await messageBytes.WriteAsync(buffer[..received.Count], CancellationToken.None);
		} while (!received.EndOfMessage);

		return new GatewayReceivedMessage(messageBytes.ToArray(), received.MessageType);
	}

	[MemberNotNull(nameof(_webSocket))]
	private void ThrowIfNullWebSocket()
	{
		if (_webSocket is null)
		{
			throw new InvalidOperationException("A websocket for this session has not been assigned.");
		}
	}

	[MemberNotNull(nameof(_webSocket))]
	private void ThrowIfWebSocketNotOpen()
	{
		ThrowIfNullWebSocket();
		if (_webSocket.State is not WebSocketState.Open)
		{
			throw new InvalidOperationException("The websocket is not open");
		}
	}

	private sealed class GatewayReceivedMessage
	{
		internal GatewayReceivedMessage(Byte[] content, WebSocketMessageType messageType)
		{
			Content = content;
			Type = messageType;
			ReceivedSuccessfully = true;
		}

		private GatewayReceivedMessage(WebSocketCloseStatus closeStatus)
		{
			CloseStatus = closeStatus;
			ReceivedSuccessfully = false;
		}

		internal static GatewayReceivedMessage Close { get; } = new([], WebSocketMessageType.Close);

		internal static GatewayReceivedMessage InvalidMessageType { get; } = new(WebSocketCloseStatus.InvalidMessageType);

		internal static GatewayReceivedMessage MessageTooBig { get; } = new(WebSocketCloseStatus.MessageTooBig);

		[MemberNotNullWhen(true, nameof(Content), nameof(Type))]
		[MemberNotNullWhen(false, nameof(CloseStatus))]
		internal Boolean ReceivedSuccessfully { get; }

		internal Byte[]? Content { get; }

		internal WebSocketMessageType? Type { get; }

		internal WebSocketCloseStatus? CloseStatus { get; }
	}
}
