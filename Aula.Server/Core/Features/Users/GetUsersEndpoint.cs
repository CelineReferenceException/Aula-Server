using Aula.Server.Core.Authorization;
using Aula.Server.Core.Endpoints;
using Aula.Server.Core.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Core.Features.Users;

internal sealed class GetUsersEndpoint : IEndpoint
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
			.RequireAuthenticatedUser()
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<Ok<List<UserData>>, ProblemHttpResult>> HandleAsync(
		[FromQuery(Name = TypeQueryParameter)] UserType? userType,
		[FromQuery(Name = CountQueryParameter)] Int32? count,
		[FromQuery(Name = AfterQueryParameter)] Snowflake? afterId,
		[FromServices] ApplicationDbContext dbContext)
	{
		count ??= DefaultUserCount;
		if (count is < MinimumUserCount or > MaximumUserCount)
		{
			return TypedResults.Problem(ProblemDetailsDefaults.InvalidUserCount);
		}

		var userQuery = dbContext.Users
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
			if (!await dbContext.Users.AnyAsync(m => m.Id == afterId && !m.IsRemoved))
			{
				return TypedResults.Problem(ProblemDetailsDefaults.InvalidAfterUser);
			}

			userQuery = userQuery.Where(m => m.Id > afterId);
		}

		return TypedResults.Ok(await userQuery.ToListAsync());
	}
}
