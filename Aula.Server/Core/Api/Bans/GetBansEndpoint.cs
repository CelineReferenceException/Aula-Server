using Aula.Server.Common.Authorization;
using Aula.Server.Common.Persistence;
using Aula.Server.Domain;
using Aula.Server.Domain.Bans;
using Aula.Server.Domain.Users;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Aula.Server.Core.Api.Bans;

internal sealed class GetBansEndpoint : IEndpoint
{
	internal const String TypeQueryParameter = "type";
	internal const String AfterQueryParameter = "after";
	internal const String CountQueryParameter = "count";
	internal const Int32 MinimumBanCount = 1;
	internal const Int32 MaximumBanCount = 100;
	internal const Int32 DefaultBanCount = 10;

	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapGet("bans", HandleAsync)
			.RequireAuthenticatedUser()
			.RequirePermissions(Permissions.BanUsers)
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<Ok<List<BanData>>, ProblemHttpResult>> HandleAsync(
		[FromQuery(Name = TypeQueryParameter)] BanType? banType,
		[FromQuery(Name = AfterQueryParameter)] Snowflake? afterId,
		[FromQuery(Name = CountQueryParameter)] Int32? count,
		[FromServices] ApplicationDbContext dbContext)
	{
		count ??= DefaultBanCount;
		if (count is > MaximumBanCount or < MinimumBanCount)
		{
			return TypedResults.Problem(ProblemDetailsDefaults.InvalidBanCount);
		}

		var bansQuery = dbContext.Bans
			.OrderBy(r => r.CreationDate)
			.Select(b => new BanData
			{
				Type = b.Type,
				ExecutorId = b.ExecutorId,
				Reason = b.Reason,
				TargetId = b.TargetId,
				CreationDate = b.CreationDate,
			})
			.Take((Int32)count);

		if (banType is not null)
		{
			bansQuery = bansQuery.Where(u => u.Type == banType);
		}

		if (afterId is not null)
		{
			var afterBan = await dbContext.Bans
				.Where(b => b.ExecutorId == afterId)
				.OrderByDescending(r => r.CreationDate)
				.Select(b => new
				{
					b.CreationDate,
				})
				.FirstOrDefaultAsync();
			if (afterBan is null)
			{
				return TypedResults.Problem(ProblemDetailsDefaults.InvalidAfterUser);
			}

			bansQuery = bansQuery.Where(b => b.CreationDate > afterBan.CreationDate);
		}


		return TypedResults.Ok(await bansQuery.ToListAsync());
	}
}
