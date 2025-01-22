using System.Net.WebSockets;

namespace WhiteTale.Server.Features.Gateway;

internal sealed class GatewayConnection : IDisposable
{
	internal GatewayConnection(UInt64? userId, Intents intents, WebSocket webSocket)
	{
		UserId = userId;
		Intents = intents;
		WebSocket = webSocket;
	}

	internal UInt64? UserId { get; init; }

	internal Intents Intents { get; init; }

	internal WebSocket WebSocket { get; init; }

	internal async Task WaitForCloseAsync()
	{
		try
		{
			var toDiscard = new Byte[1024].AsMemory();
			do
			{
				_ = await WebSocket.ReceiveAsync(toDiscard, CancellationToken.None);
			} while (WebSocket.State is not WebSocketState.CloseReceived);

			await WebSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, String.Empty, CancellationToken.None);
		}
		catch (WebSocketException)
		{
			// Prevents stopping the work if the connection closes ungracefully.
		}
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(Boolean disposing)
	{
		if (disposing)
		{
			WebSocket.Dispose();
		}
	}

	~GatewayConnection()
	{
		Dispose(false);
	}
}
