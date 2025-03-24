using Asp.Versioning;
using Aula.Server.Core.Authorization;
using Aula.Server.Core.Endpoints;
using Aula.Server.Core.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Aula.Server.Core.Features.Bots;

internal sealed class CreateBotEndpoint : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapPost("bots", HandleAsync)
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

		var user = User.Create(await snowflakeGenerator.NewSnowflakeAsync(), Guid.CreateVersion7().ToString(), null, body.DisplayName,
				String.Empty, UserType.Bot, Permissions.None)
			.Value!;

		var registerResult = await userManager.RegisterAsync(user);
		if (!registerResult.Succeeded)
		{
			return TypedResults.Problem(new ProblemDetails
			{
				Title = "Register problem",
				Detail = registerResult.Description,
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
