using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using WhiteTale.Server.Features.Users.Gateway;

namespace WhiteTale.Server.Features.Gateway.Endpoints;

internal sealed class ConnectToGateway : IEndpoint
{
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
		[FromServices] GatewayService gatewayService)
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

		GatewaySession session;

		if (sessionId is not null)
		{
			if (!gatewayService.Sessions.TryGetValue(sessionId, out var previousSession) ||
			    previousSession.UserId != userId ||
			    previousSession.IsActive ||
			    previousSession.CloseTime < DateTime.UtcNow - gatewayService.TimeToExpire)
			{
				return TypedResults.BadRequest();
			}

			var socket = await httpContext.WebSockets.AcceptWebSocketAsync();
			previousSession.SetWebSocket(socket);

			session = previousSession;
		}
		else
		{
			var socket = await httpContext.WebSockets.AcceptWebSocketAsync();
			session = gatewayService.CreateSession(userId, intents);
			session.SetWebSocket(socket);
		}

		await session.RunAsync(presence ?? PresenceOptions.Online);

		return TypedResults.Empty;
	}
}
