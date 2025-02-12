using System.Net;
using Aula.Server.Common.Endpoints;
using Aula.Server.Common.Identity;
using Aula.Server.Common.RateLimiting;
using Aula.Server.Domain.Users;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Aula.Server.Features.Identity.Endpoints;

internal sealed class ForgotPassword : IEndpoint
{
	private const String EmailQueryParameter = "email";

	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapPost("identity/forgot-password", HandleAsync)
			.RequireRateLimiting(RateLimitPolicyNames.Global)
			.HasApiVersion(1);
	}

	private static async Task<NoContent> HandleAsync(
		[FromQuery(Name = EmailQueryParameter)] String email,
		[FromServices] UserManager userManager,
		[FromServices] ResetPasswordEmailSender resetPasswordEmailSender)
	{
		email = WebUtility.UrlDecode(email);

		var user = await userManager.FindByEmailAsync(email);
		if (user?.Type is not UserType.Standard)
		{
			// We could return NotFound, but it feels like unnecessary information.
			return TypedResults.NoContent();
		}

		await resetPasswordEmailSender.SendEmailAsync(user);
		return TypedResults.NoContent();
	}
}
