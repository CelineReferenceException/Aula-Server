using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace WhiteTale.Server.Features.Users.Endpoints;

internal sealed class GetAllUsers : IEndpoint
{
	private const String TypeQueryParameter = "type";

	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapGet("users", HandleAsync)
			.RequireRateLimiting(RateLimitPolicyNames.Global)
			.RequireAuthenticatedUser()
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Ok<List<UserData>>> HandleAsync(
		[FromQuery(Name = TypeQueryParameter)] UserType? userType,
		[FromServices] ApplicationDbContext dbContext)
	{
		var userQuery = dbContext.Users
			.AsNoTracking()
			.Where(u => !u.IsRemoved)
			.Select(u => new UserData
			{
				Id = u.Id,
				DisplayName = u.DisplayName,
				Description = u.Description,
				Type = u.Type,
				Presence = u.Presence,
				Permissions = u.Permissions,
				CurrentRoomId = u.CurrentRoomId,
			});
		if (userType is not null)
		{
			userQuery = userQuery.Where(u => u.Type == userType);
		}

		return TypedResults.Ok(await userQuery.ToListAsync());
	}
}
