﻿using Aula.Server.Core.Endpoints;
using Aula.Server.Core.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;

namespace Aula.Server.Core.Features.Identity;

internal sealed class RegisterEndpoint : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapPost("identity/register", HandleAsync)
			.HasApiVersion(1);
	}

	private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
		[FromBody] RegisterRequestBody body,
		[FromServices] RegisterRequestBodyValidator bodyValidator,
		[FromServices] SnowflakeGenerator snowflakeGenerator,
		[FromServices] UserManager userManager,
		[FromServices] PasswordHasher<User> passwordHasher,
		[FromServices] IOptions<IdentityFeatureOptions> featureOptions,
		[FromServices] ConfirmEmailEmailSender confirmEmailEmailSender,
		[FromServices] ResetPasswordEmailSender resetPasswordEmailSender)
	{
		var bodyValidation = await bodyValidator.ValidateAsync(body);
		if (!bodyValidation.IsValid)
		{
			var problemDetails = bodyValidation.Errors.ToProblemDetails();
			return TypedResults.Problem(problemDetails);
		}

		var currentUser = await userManager.FindByEmailAsync(body.Email);
		if (currentUser is not null)
		{
			await resetPasswordEmailSender.SendEmailAsync(currentUser);
			return TypedResults.NoContent();
		}

		var newUser = new User(await snowflakeGenerator.NewSnowflakeAsync(), body.UserName, body.Email, body.DisplayName, "",
			UserType.Standard, featureOptions.Value.DefaultPermissions);
		newUser.PasswordHash = passwordHasher.HashPassword(newUser, body.Password);

		var registerResult = await userManager.RegisterAsync(newUser);
		if (!registerResult.Succeeded)
		{
			return TypedResults.Problem(new ProblemDetails
			{
				Title = "Register problem",
				Detail = registerResult.Description,
				Status = StatusCodes.Status400BadRequest,
			});
		}

		await confirmEmailEmailSender.SendEmailAsync(newUser);
		return TypedResults.NoContent();
	}
}
