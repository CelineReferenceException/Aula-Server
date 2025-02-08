using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace WhiteTale.Server.Features.Users.Endpoints;

internal sealed class ModifyOwnUser : IEndpoint
{
	public void Build(IEndpointRouteBuilder builder)
	{
		_ = builder.MapPatch("users/@me", HandleAsync)
			.RequireRateLimiting(RateLimitPolicyNames.Global)
			.RequireAuthorization(IdentityAuthorizationPolicyNames.BearerToken)
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<Ok<UserData>, ProblemHttpResult, InternalServerError>> HandleAsync(
		[FromBody] ModifyOwnUserRequestBody body,
		HttpContext httpContext,
		[FromServices] UserManager userManager,
		[FromServices] ModifyOwnUserRequestBodyValidator bodyValidator,
		[FromServices] ApplicationDbContext dbContext,
		[FromServices] ILogger<ModifyOwnUser> logger)
	{
		var bodyValidation = await bodyValidator.ValidateAsync(body);
		if (!bodyValidation.IsValid)
		{
			var problemDetails = bodyValidation.Errors.ToProblemDetails();
			return TypedResults.Problem(problemDetails);
		}

		var userId = userManager.GetUserId(httpContext.User);
		if (userId is null)
		{
			return TypedResults.InternalServerError();
		}

		var user = await dbContext.Users
			.AsTracking()
			.Where(u => u.Id == userId)
			.FirstOrDefaultAsync();
		if (user is null)
		{
			return TypedResults.InternalServerError();
		}

		user.Modify(body.DisplayName, body.Description);
		user.UpdateConcurrencyStamp();

		try
		{
			_ = await dbContext.SaveChangesAsync();
		}
		catch (DbUpdateConcurrencyException)
		{
			return TypedResults.InternalServerError();
		}

		return TypedResults.Ok(new UserData
		{
			Id = user.Id,
			DisplayName = user.DisplayName,
			Description = user.Description,
			CurrentRoomId = user.CurrentRoomId,
			Presence = user.Presence,
			OwnerType = user.OwnerType,
			Permissions = user.Permissions,
		});
	}
}
