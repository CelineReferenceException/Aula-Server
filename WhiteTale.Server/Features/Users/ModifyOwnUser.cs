using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace WhiteTale.Server.Features.Users;

internal sealed class ModifyOwnUser : IEndpoint
{
	public void Build(IEndpointRouteBuilder builder)
	{
		_ = builder.MapPatch("api/users/@me", HandleAsync)
			.RequireRateLimiting(CommonRateLimitPolicyNames.Global)
			.RequireAuthorization(IdentityAuthorizationPolicyNames.BearerToken);
	}

	private static async Task<Results<Ok<UserData>, ProblemHttpResult, InternalServerError>> HandleAsync(
		[FromBody] ModifyOwnUserRequestBody body,
		HttpContext httpContext,
		[FromServices] UserManager<User> userManager,
		[FromServices] IValidator<ModifyOwnUserRequestBody> bodyValidator,
		[FromServices] ApplicationDbContext dbContext,
		[FromServices] ILogger<ModifyOwnUser> logger)
	{
		var bodyValidation = await bodyValidator.ValidateAsync(body);
		if (!bodyValidation.IsValid)
		{
			var problemDetails = bodyValidation.Errors.ToProblemDetails();
			return TypedResults.Problem(problemDetails);
		}

		var userIdClaimValue = userManager.GetUserId(httpContext.User);
		if (!UInt64.TryParse(userIdClaimValue, out var userId))
		{
			return TypedResults.InternalServerError();
		}

		var user = await dbContext.Users
			.AsTracking()
			.Where(x => x.Id == userId)
			.FirstOrDefaultAsync();
		if (user is null)
		{
			return TypedResults.InternalServerError();
		}

		user.Modify(body.DisplayName, body.Description);

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
