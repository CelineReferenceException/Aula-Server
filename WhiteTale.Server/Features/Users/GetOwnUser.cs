using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace WhiteTale.Server.Features.Users;

internal sealed class GetOwnUser : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapGet("users/@me", HandleAsync)
			.RequireRateLimiting(RateLimitPolicyNames.Global)
			.RequireAuthorization(IdentityAuthorizationPolicyNames.BearerToken)
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<Ok<UserData>, InternalServerError>> HandleAsync(
		HttpContext httpContext,
		[FromServices] UserManager<User> userManager)
	{
		var user = await userManager.GetUserAsync(httpContext.User);
		if (user is null)
		{
			return TypedResults.InternalServerError();
		}

		return TypedResults.Ok(new UserData
		{
			Id = user.Id,
			DisplayName = user.DisplayName,
			Description = user.Description,
			CurrentRoomId = user.CurrentRoomId,
			OwnerType = user.OwnerType,
			Presence = user.Presence,
			Permissions = user.Permissions,
		});
	}
}
