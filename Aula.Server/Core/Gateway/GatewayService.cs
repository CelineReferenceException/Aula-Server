using System.Collections.Concurrent;
using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

namespace Aula.Server.Core.Gateway;

internal sealed class GatewayService
{
	private readonly JsonSerializerOptions _jsonSerializerOptions;
	private readonly IServiceScope _serviceScope;
	private readonly ConcurrentDictionary<String, GatewaySession> _sessions = [];

	public GatewayService(IOptions<GatewayOptions> gatewayOptions, IOptions<JsonOptions> jsonOptions, IServiceScopeFactory scopeFactory)
	{
		_jsonSerializerOptions = jsonOptions.Value.SerializerOptions;
		ExpirePeriod = TimeSpan.FromSeconds(gatewayOptions.Value.SecondsToExpire);
		_serviceScope = scopeFactory.CreateScope();
	}

	internal TimeSpan ExpirePeriod { get; }

	internal IReadOnlyDictionary<String, GatewaySession> Sessions => _sessions;

	internal GatewaySession CreateSession(
		UInt64 userId,
		Intents intents)
	{
		var session = new GatewaySession(userId,
			intents,
			_jsonSerializerOptions,
			_serviceScope.ServiceProvider.GetRequiredService<IPublisher>());

		_ = _sessions.TryAdd(session.Id, session);
		return session;
	}

	internal void ClearExpiredSessions()
	{
		var expiredSessions = _sessions.Values.Where(s => s.CloseDate < DateTime.UtcNow - ExpirePeriod);
		foreach (var session in expiredSessions)
		{
			_ = _sessions.TryRemove(session.Id, out _);
		}
	}
}
