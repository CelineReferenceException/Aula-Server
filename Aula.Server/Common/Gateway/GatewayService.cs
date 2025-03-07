using System.Collections.Concurrent;
using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

namespace Aula.Server.Common.Gateway;

internal sealed class GatewayService : IDisposable
{
	private readonly JsonSerializerOptions _jsonSerializerOptions;
	private readonly IPublisher _publisher;
	private readonly IServiceScope _serviceScope;
	private readonly ConcurrentDictionary<String, GatewaySession> _sessions = [];

	public GatewayService(IOptions<GatewayOptions> gatewayOptions, IOptions<JsonOptions> jsonOptions, IServiceScopeFactory scopeFactory)
	{
		_jsonSerializerOptions = jsonOptions.Value.SerializerOptions;
		_serviceScope = scopeFactory.CreateScope();
		_publisher = _serviceScope.ServiceProvider.GetRequiredService<IPublisher>();
		TimeToExpire = TimeSpan.FromSeconds(gatewayOptions.Value.SecondsToExpire);
	}

	internal TimeSpan TimeToExpire { get; }

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
