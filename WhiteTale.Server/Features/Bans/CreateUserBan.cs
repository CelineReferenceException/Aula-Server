using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace WhiteTale.Server.Features.Bans;

internal sealed class CreateUserBan : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapPost("bans/users/{targetId}", HandleAsync)
			.RequireAuthorization(IdentityAuthorizationPolicyNames.BearerToken)
			.RequirePermissions(Permissions.BanUsers)
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<Ok<BanData>, ProblemHttpResult, InternalServerError>> HandleAsync(
		[FromRoute] UInt64 targetId,
		[FromBody] CreateBanRequestBody body,
		[FromServices] CreateBanRequestBodyValidator bodyValidator,
		HttpContext httpContext,
		[FromServices] UserManager<User> userManager,
		[FromServices] ApplicationDbContext dbContext,
		[FromServices] SnowflakeGenerator snowflakeGenerator)
	{
		var validation = await bodyValidator.ValidateAsync(body);
		if (!validation.IsValid)
		{
			var problemDetails = validation.Errors.ToProblemDetails();
			return TypedResults.Problem(problemDetails);
		}

		var userIdClaimValue = userManager.GetUserId(httpContext.User);
		if (!UInt64.TryParse(userIdClaimValue, out var userId))
		{
			return TypedResults.InternalServerError();
		}

		var currentBan = await dbContext.Bans
			.AsTracking()
			.Where(b => b.TargetId == targetId)
			.FirstOrDefaultAsync();
		if (currentBan is not null)
		{
			return TypedResults.Problem(new ProblemDetails
			{
				Title = "Ban already exists",
				Detail = "This user is already banned.",
				Status = StatusCodes.Status409Conflict,
			});
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

		return TypedResults.Ok(new BanData
		{
			Type = ban.Type,
			ExecutorId = ban.ExecutorId,
			Reason = ban.Reason,
			TargetId = ban.TargetId,
			CreationTime = ban.CreationTime,
		});
	}
}
