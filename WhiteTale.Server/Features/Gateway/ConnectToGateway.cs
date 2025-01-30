using System.Collections.Concurrent;
using System.Net.WebSockets;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using WhiteTale.Server.Features.Gateway.Events.Receive.Presences;

namespace WhiteTale.Server.Features.Gateway;

internal sealed class ConnectToGateway : IEndpoint
{
	private static readonly TimeSpan s_timeToExpireSession = TimeSpan.FromSeconds(60);

	private static readonly ConcurrentDictionary<String, GatewaySession> s_sessions = [];

	internal static IReadOnlyDictionary<String, GatewaySession> Sessions => s_sessions;

	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.Map("gateway", HandleAsync)
			.RequireRateLimiting(GatewayRateLimitPolicyNames.Default)
			.RequireAuthorization(IdentityAuthorizationPolicyNames.BearerToken)
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<EmptyHttpResult, BadRequest, InternalServerError>> HandleAsync(
		HttpContext httpContext,
		[FromHeader(Name = "X-Intents")] Intents intents,
		[FromHeader(Name = "X-SessionId")] String? sessionId,
		[FromHeader(Name = "X-Presence")] PresenceOptions? presence,
		[FromServices] UserManager<User> userManager,
		[FromServices] IPublisher publisher,
		[FromServices] IOptions<JsonOptions> jsonOptions)
	{
		if (!httpContext.WebSockets.IsWebSocketRequest)
		{
			return TypedResults.BadRequest();
		}

		var userIdClaimValue = userManager.GetUserId(httpContext.User);
		if (!UInt64.TryParse(userIdClaimValue, out var userId))
		{
			return TypedResults.InternalServerError();
		}

		WebSocket socket;
		GatewaySession session;

		if (sessionId is not null)
		{
			if (!s_sessions.TryGetValue(sessionId, out var previousSession) ||
			    previousSession.UserId != userId ||
			    previousSession.IsActive ||
			    previousSession.CloseTime < DateTime.UtcNow - s_timeToExpireSession)
			{
				return TypedResults.BadRequest();
			}

			socket = await httpContext.WebSockets.AcceptWebSocketAsync();
			previousSession.SetWebSocket(socket);
			session = previousSession;
		}
		else
		{
			socket = await httpContext.WebSockets.AcceptWebSocketAsync();
			session = new GatewaySession(userId, intents, socket, jsonOptions.Value.JsonSerializerOptions);
			_ = s_sessions.TryAdd(session.Id, session);
		}

		await session.RunAsync(publisher, presence ?? PresenceOptions.Online);
		socket.Dispose();

		return TypedResults.Empty;
	}

	internal static void RemoveExpiredSessions()
	{
		var expiredSessions = s_sessions.Values
			.Where(s => s.CloseTime < DateTime.UtcNow - s_timeToExpireSession);
		foreach (var session in expiredSessions)
		{
			s_sessions.TryRemove(session.Id, out _);
		}
	}
}
