using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading.Channels;
using Aula.Server.Features.Users.Gateway;
using MediatR;

namespace Aula.Server.Common.Gateway;

internal sealed class GatewaySession : IDisposable
{
	private readonly Channel<Byte[]> _eventsQueue = Channel.CreateUnbounded<Byte[]>();
	private readonly JsonSerializerOptions _eventsReceivedJsonOptions;
	private readonly IPublisher _publisher;
	private Boolean _disposed;
	private Boolean _isFirstConnect = true;
	private Boolean _running;
	private Byte[]? _nextEvent;
	private CancellationTokenSource _sendingEventsCancellation = null!;
	private WebSocket? _webSocket;

	internal GatewaySession(
		UInt64 userId,
		Intents intents,
		JsonSerializerOptions eventsReceivedJsonOptions,
		IPublisher publisher)
	{
		UserId = userId;
		Intents = intents;
		_eventsReceivedJsonOptions = eventsReceivedJsonOptions;
		_publisher = publisher;
		Id = Guid.NewGuid().ToString("N");
	}

	internal Boolean IsActive => _running;

	internal String Id { get; }

	internal DateTime? CloseDate { get; private set; }

	internal UInt64 UserId { get; }

	internal Intents Intents { get; }

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	internal async Task RunAsync(PresenceOptions presence)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		ThrowIfNullWebSocket();

		if (Interlocked.Exchange(ref _running, true))
		{
			throw new InvalidOperationException("Gateway is already running.");
		}

		if (_webSocket.State is not (WebSocketState.Open or WebSocketState.Connecting))
		{
			throw new InvalidOperationException("WebSocket is not available.");
		}

		CloseDate = null;
		_sendingEventsCancellation = new CancellationTokenSource();

		var receiving = StartReceivingEventsAsync(_publisher);
		var sending = StartSendingEventsAsync();

		if (_isFirstConnect)
		{
			await _publisher.Publish(new HelloEvent
			{
				Session = this,
			});

			_isFirstConnect = false;
		}

		await _publisher.Publish(new GatewayConnectedEvent
		{
			Session = this,
			Presence = presence,
		});

		await Task.WhenAll(receiving, sending);

		await _publisher.Publish(new GatewayDisconnectedEvent
		{
			Session = this,
		});

		CloseDate = DateTime.UtcNow;
		_running = false;
	}

	internal async Task StopAsync(WebSocketCloseStatus closeStatus)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		ThrowIfNullWebSocket();

		if (!_running)
		{
			return;
		}

		if (!_sendingEventsCancellation.IsCancellationRequested)
		{
			await _sendingEventsCancellation.CancelAsync();
			_sendingEventsCancellation.Dispose();
		}

		if (_webSocket.State is WebSocketState.Open or WebSocketState.CloseReceived)
		{
			await _webSocket.CloseOutputAsync(closeStatus, String.Empty, CancellationToken.None);
			_webSocket.Dispose();
		}
	}

	private async Task StartSendingEventsAsync()
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		ThrowIfNullWebSocket();
		try
		{
			while (true)
			{
				if (_nextEvent is null)
				{
					_ = await _eventsQueue.Reader.WaitToReadAsync(_sendingEventsCancellation.Token);
					_nextEvent = await _eventsQueue.Reader.ReadAsync(_sendingEventsCancellation.Token);
				}

				await _webSocket.SendAsync(_nextEvent.AsMemory(), WebSocketMessageType.Text, true, _sendingEventsCancellation.Token);

				_nextEvent = null;
			}
		}
		catch (OperationCanceledException)
		{
		}
	}

	private async Task StartReceivingEventsAsync(IPublisher publisher)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		ThrowIfNullWebSocket();
		try
		{
			var buffer = new Byte[1024 / 4].AsMemory();
			do
			{
				using var payloadBytes = new MemoryStream();
				ValueWebSocketReceiveResult received;
				do
				{
					if (payloadBytes.Length > 1024 * 4)
					{
						await StopAsync(WebSocketCloseStatus.MessageTooBig);
						return;
					}

					received = await _webSocket.ReceiveAsync(buffer, CancellationToken.None);
					await payloadBytes.WriteAsync(buffer[..received.Count], CancellationToken.None);
				} while (!received.EndOfMessage);

				_ = payloadBytes.Seek(0, SeekOrigin.Begin);
				var payload = await JsonSerializer.DeserializeAsync<GatewayPayload>(payloadBytes, _eventsReceivedJsonOptions);
				if (payload is null)
				{
					await StopAsync(WebSocketCloseStatus.InvalidPayloadData);
					return;
				}

				await publisher.Publish(new PayloadReceivedEvent
				{
					Session = this,
					Payload = payload,
				});
			} while (_webSocket.State is WebSocketState.Open);

			await StopAsync(WebSocketCloseStatus.NormalClosure);
		}
		catch (Exception ex) when (ex is WebSocketException or JsonException)
		{
			await StopAsync(WebSocketCloseStatus.InternalServerError);
		}
	}

	internal async Task QueueEventAsync(Byte[] payload, CancellationToken cancellationToken = default)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);

		_ = await _eventsQueue.Writer.WaitToWriteAsync(cancellationToken);
		await _eventsQueue.Writer.WriteAsync(payload, cancellationToken);
	}

	internal void SetWebSocket(WebSocket webSocket)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		ArgumentNullException.ThrowIfNull(webSocket, nameof(webSocket));
		if (_running)
		{
			throw new InvalidOperationException("Cannot set the websocket while the gateway is running.");
		}

		_webSocket = webSocket;
	}

	private void Dispose(Boolean disposing)
	{
		if (_disposed)
		{
			return;
		}

		_disposed = true;

		if (disposing)
		{
			_sendingEventsCancellation.Dispose();
		}
	}

	~GatewaySession()
	{
		Dispose(false);
	}

	[MemberNotNull(nameof(_webSocket))]
	private void ThrowIfNullWebSocket()
	{
		if (_webSocket is null)
		{
			throw new InvalidOperationException("A websocket for this session has not been assigned.");
		}
	}
}
