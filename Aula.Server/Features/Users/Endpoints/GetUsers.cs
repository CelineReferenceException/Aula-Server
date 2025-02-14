using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Features.Users.Endpoints;

internal sealed class GetUsers : IEndpoint
{
	private const String TypeQueryParameter = "type";
	private const String CountQueryParameter = "count";
	internal const String AfterQueryParameter = "after";
	internal const Int32 MinimumUserCount = 2;
	internal const Int32 MaximumUserCount = 100;
	private const Int32 DefaultUserCount = 10;

	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapGet("users", HandleAsync)
			.RequireRateLimiting(RateLimitPolicyNames.Global)
			.RequireAuthenticatedUser()
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<Ok<List<UserData>>, ProblemHttpResult>> HandleAsync(
		[FromQuery(Name = TypeQueryParameter)] UserType? userType,
		[FromQuery(Name = CountQueryParameter)] Int32? count,
		[FromQuery(Name = AfterQueryParameter)] UInt64? afterId,
		[FromServices] ApplicationDbContext dbContext)
	{
		count ??= DefaultUserCount;
		if (count is < MinimumUserCount or > MaximumUserCount)
		{
			return TypedResults.Problem(ProblemDetailsDefaults.InvalidUserCount);
		}

		var userQuery = dbContext.Users
			.AsNoTracking()
			.Where(u => !u.IsRemoved)
			.OrderBy(u => u.Id)
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

		if (afterId is not null)
		{
			var targetExists = await dbContext.Users
				.AsNoTracking()
				.AnyAsync(m => m.Id == afterId && !m.IsRemoved);

			if (!targetExists)
			{
				return TypedResults.Problem(ProblemDetailsDefaults.InvalidAfterUser);
			}

			userQuery = userQuery.Where(m => m.Id > afterId);
		}

		return TypedResults.Ok(await userQuery.ToListAsync());
	}
}
