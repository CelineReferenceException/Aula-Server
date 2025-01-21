using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace WhiteTale.Server.Features.Identity;

internal sealed class ForgotPassword : IEndpoint
{
	private const String EmailQueryParameter = "email";

	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapPost("api/identity/forgotpassword", HandleAsync)
			.RequireRateLimiting(CommonRateLimitPolicyNames.Global);
	}

	private static async Task<NoContent> HandleAsync(
		[FromQuery(Name = EmailQueryParameter)] String email,
		[FromServices] UserManager<User> userManager,
		[FromServices] ResetPasswordEmailSender resetPasswordEmailSender)
	{
		email = WebUtility.UrlDecode(email);

		var user = await userManager.FindByEmailAsync(email);
		if (user is null)
		{
			// We could return NotFound, but it feels like unnecessary information.
			return TypedResults.NoContent();
		}

		await resetPasswordEmailSender.SendEmailAsync(user);
		return TypedResults.NoContent();
	}
}
