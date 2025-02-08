using System.Collections.Concurrent;
using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

namespace WhiteTale.Server.Common.Gateway;

internal sealed class GatewayService : IDisposable
{
	private readonly JsonSerializerOptions _jsonSerializerOptions;
	private readonly IPublisher _publisher;
	private readonly IServiceScope _serviceScope;
	private readonly ConcurrentDictionary<String, GatewaySession> _sessions = [];

	public GatewayService(IOptions<JsonOptions> jsonOptions, IServiceProvider serviceProvider)
	{
		_jsonSerializerOptions = jsonOptions.Value.SerializerOptions;
		_serviceScope = serviceProvider.CreateScope();
		_publisher = _serviceScope.ServiceProvider.GetRequiredService<IPublisher>();
	}

	internal TimeSpan TimeToExpire { get; } = TimeSpan.FromSeconds(60);

	internal IReadOnlyDictionary<String, GatewaySession> Sessions => _sessions;

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	internal GatewaySession CreateSession(
		UInt64 userId,
		Intents intents)
	{
		var session = new GatewaySession(userId, intents, _jsonSerializerOptions, _publisher);
		_ = _sessions.TryAdd(session.Id, session);

		return session;
	}

	internal void RemoveExpiredSessions()
	{
		var expiredSessions = _sessions.Values
			.Where(s => s.CloseTime < DateTime.UtcNow - TimeToExpire);
		foreach (var session in expiredSessions)
		{
			_ = _sessions.TryRemove(session.Id, out _);
		}
	}

	private void Dispose(Boolean disposing)
	{
		if (disposing)
		{
			_serviceScope.Dispose();
		}
	}

	~GatewayService()
	{
		Dispose(false);
	}
}
