using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace WhiteTale.Server.Features.Bans.Endpoints;

internal sealed class BanUser : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapPut("bans/users/{targetId}", HandleAsync)
			.RequireAuthenticatedUser()
			.RequirePermissions(Permissions.BanUsers)
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<Created<BanData>, ProblemHttpResult, InternalServerError>> HandleAsync(
		[FromRoute] UInt64 targetId,
		[FromBody] CreateBanRequestBody body,
		[FromServices] CreateBanRequestBodyValidator bodyValidator,
		HttpContext httpContext,
		[FromServices] UserManager userManager,
		[FromServices] ApplicationDbContext dbContext,
		[FromServices] SnowflakeGenerator snowflakeGenerator)
	{
		var validation = await bodyValidator.ValidateAsync(body);
		if (!validation.IsValid)
		{
			var problemDetails = validation.Errors.ToProblemDetails();
			return TypedResults.Problem(problemDetails);
		}

		var userId = userManager.GetUserId(httpContext.User);
		if (userId is null)
		{
			return TypedResults.InternalServerError();
		}

		var currentBan = await dbContext.Bans
			.AsTracking()
			.Where(b => b.TargetId == targetId)
			.FirstOrDefaultAsync();
		if (currentBan is not null)
		{
			return TypedResults.Problem(ProblemDetailsDefaults.UserAlreadyBanned);
		}

		var targetUser = await dbContext.Users
			.AsNoTracking()
			.Where(u => u.Id == targetId)
			.Select(u => new
			{
				u.Permissions,
			})
			.FirstOrDefaultAsync();
		if (targetUser is null)
		{
			return TypedResults.Problem(ProblemDetailsDefaults.TargetDoesNotExist);
		}

		if (targetUser.Permissions.HasFlag(Permissions.Administrator))
		{
			return TypedResults.Problem(ProblemDetailsDefaults.TargetIsAdministrator);
		}

		var ban = Ban.Create(snowflakeGenerator.NewSnowflake(), BanType.Id, userId, body.Reason, targetId);
		_ = dbContext.Bans.Add(ban);

		try
		{
			_ = await dbContext.SaveChangesAsync();
		}
		catch (DbUpdateConcurrencyException)
		{
			return TypedResults.InternalServerError();
		}

		return TypedResults.Created(httpContext.Request.GetUrl(), new BanData
		{
			Type = ban.Type,
			ExecutorId = ban.ExecutorId,
			Reason = ban.Reason,
			TargetId = ban.TargetId,
			CreationTime = ban.CreationTime,
		});
	}
}
