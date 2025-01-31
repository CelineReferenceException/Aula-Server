using System.Collections.Concurrent;
using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

namespace WhiteTale.Server.Common.Gateway;

internal sealed class GatewayService
{
	private readonly ConcurrentDictionary<String, GatewaySession> _sessions = [];
	private readonly JsonSerializerOptions _jsonSerializerOptions;
	private readonly IPublisher _publisher;

	internal TimeSpan TimeToExpire { get; } = TimeSpan.FromSeconds(60);

	internal IReadOnlyDictionary<String, GatewaySession> Sessions => _sessions;

	public GatewayService(IOptions<JsonOptions> jsonOptions, IPublisher publisher)
	{
		_jsonSerializerOptions = jsonOptions.Value.SerializerOptions;
		_publisher = publisher;
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
}
