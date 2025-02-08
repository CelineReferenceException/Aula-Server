using System.Net;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace WhiteTale.Server.Features.Identity.Endpoints;

internal sealed class ConfirmEmail : IEndpoint
{
	internal const String EmailQueryParameter = "email";
	internal const String TokenQueryParameter = "token";

	public void Build(IEndpointRouteBuilder route)
	{
		// Using GET allows browsers to make direct requests, such as when a user enters a URL in the address bar.
		_ = route.MapGet("identity/confirmEmail", HandleAsync)
			.RequireRateLimiting(RateLimitPolicyNames.Global)
			.HasApiVersion(1);
	}

	private static async Task<Results<NoContent, RedirectHttpResult>> HandleAsync(
		[FromQuery(Name = EmailQueryParameter)] String email,
		[FromQuery(Name = TokenQueryParameter)] String? token,
		[FromServices] UserManager userManager,
		HttpRequest httpRequest,
		[FromServices] ConfirmEmailEmailSender confirmEmailEmailSender,
		[FromServices] IOptions<IdentityFeatureOptions> featureOptions)
	{
		email = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(email));
		var redirectUri = featureOptions.Value.ConfirmEmailRedirectUri?.ToString();

		var user = await userManager.FindByEmailAsync(email);
		if (user is null)
		{
			// We could return NotFound, but it feels like unnecessary information.
			return RedirectOrSendNoContent(redirectUri);
		}

		if (user.EmailConfirmed)
		{
			return RedirectOrSendNoContent(redirectUri);
		}

		if (token is null)
		{
			await confirmEmailEmailSender.SendEmailAsync(user, httpRequest);
			return RedirectOrSendNoContent(redirectUri);
		}

		try
		{
			token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
		}
		catch (FormatException)
		{
			return RedirectOrSendNoContent(redirectUri);
		}

		_ = await userManager.ConfirmEmailAsync(user, token);

		return RedirectOrSendNoContent(redirectUri);
	}

	private static Results<NoContent, RedirectHttpResult> RedirectOrSendNoContent(String? redirectUri)
	{
		return redirectUri is not null ? TypedResults.Redirect(redirectUri) : TypedResults.NoContent();
	}
}
