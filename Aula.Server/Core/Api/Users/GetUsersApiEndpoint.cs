using Aula.Server.Common.Authorization;
using Aula.Server.Common.Persistence;
using Aula.Server.Domain;
using Aula.Server.Domain.Users;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Core.Api.Users;

internal sealed class GetUsersApiEndpoint : IApiEndpoint
{
	internal const String TypeQueryParameter = "type";
	internal const String CountQueryParameter = "count";
	internal const String AfterQueryParameter = "after";
	internal const Int32 MinimumUserCount = 1;
	internal const Int32 MaximumUserCount = 100;
	internal const Int32 DefaultUserCount = 10;

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
			})
			.Take((Int32)count);
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
