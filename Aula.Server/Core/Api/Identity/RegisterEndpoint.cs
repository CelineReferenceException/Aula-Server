﻿using Aula.Server.Common.Identity;
using Aula.Server.Domain.Users;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;

namespace Aula.Server.Core.Api.Identity;

internal sealed class RegisterEndpoint : IApiEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapPost("identity/register", HandleAsync)
			.HasApiVersion(1);
	}

	private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
		[FromBody] RegisterRequestBody body,
		[FromServices] IValidator<RegisterRequestBody> bodyValidator,
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

		var newUser = User.Create(await snowflakeGenerator.NewSnowflakeAsync(), body.UserName, body.Email, body.DisplayName, String.Empty,
				UserType.Standard, featureOptions.Value.DefaultPermissions)
			.Value!;
		newUser.ChangePassword(passwordHasher.HashPassword(newUser, body.Password));

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
