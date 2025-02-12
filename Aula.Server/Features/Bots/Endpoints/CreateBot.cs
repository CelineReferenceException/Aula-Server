using Asp.Versioning;
using Aula.Server.Common;
using Aula.Server.Common.Endpoints;
using Aula.Server.Common.Identity;
using Aula.Server.Common.RateLimiting;
using Aula.Server.Domain.Users;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Aula.Server.Features.Bots.Endpoints;

internal sealed class CreateBot : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapPost("bots", HandleAsync)
			.RequireRateLimiting(RateLimitPolicyNames.Global)
			.RequireAuthenticatedUser()
			.RequirePermissions()
			.DenyBannedUsers()
			.HasApiVersion(1);
	}

	private static async Task<Results<Ok<CreateBotResponse>, ProblemHttpResult, InternalServerError>> HandleAsync(
		[FromBody] CreateBotRequestBody body,
		[FromServices] CreateBotRequestBodyValidator bodyValidator,
		[FromServices] UserManager userManager,
		[FromServices] SnowflakeGenerator snowflakeGenerator,
		[FromServices] TokenProvider tokenProvider,
		[FromServices] ApiVersion apiVersion)
	{
		var validation = await bodyValidator.ValidateAsync(body);
		if (!validation.IsValid)
		{
			var problemDetails = validation.Errors.ToProblemDetails();
			return TypedResults.Problem(problemDetails);
		}

		var user = User.Create(snowflakeGenerator.NewSnowflake(), Guid.CreateVersion7().ToString(), null, body.DisplayName, UserType.Bot,
			Permissions.None);

		var registerResult = await userManager.RegisterAsync(user);
		if (!registerResult.Succeeded)
		{
			return TypedResults.Problem(new ProblemDetails
			{
				Title = "Register problem",
				Detail = registerResult.ToString(),
				Status = StatusCodes.Status400BadRequest,
			});
		}

		return TypedResults.Ok(new CreateBotResponse
		{
			User = new UserData
			{
				Id = user.Id,
				DisplayName = user.DisplayName,
				Description = user.Description,
				Type = user.Type,
				Presence = user.Presence,
				Permissions = user.Permissions,
				CurrentRoomId = user.CurrentRoomId,
			},
			Token = tokenProvider.CreateToken(user),
		});
	}
}
