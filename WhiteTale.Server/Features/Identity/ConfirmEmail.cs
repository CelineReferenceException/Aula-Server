using System.Net;
using System.Text;
using WhiteTale.Server.Domain.Users;

namespace WhiteTale.Server.Features.Identity;

internal sealed class ConfirmEmail : IEndpoint
{
	internal const String EmailQueryParameter = "email";
	internal const String TokenQueryParameter = "token";
	internal const String RedirectUriQueryParameter = "redirectUri";
	internal const String Route = "api/identity/confirmEmail";

	public void Build(IEndpointRouteBuilder route)
	{
		// Using GET allows browsers to make direct requests, such as when a user enters a URL in the address bar.
		_ = route.MapGet(Route, HandleAsync)
			.RequireRateLimiting(RateLimitPolicyNames.Global)
			.RequireRateLimiting(RateLimitPolicyNames.NoConcurrency);
	}

	private static async Task<Results<NoContent, RedirectHttpResult>> HandleAsync(
		[FromQuery(Name = EmailQueryParameter)] String email,
		[FromQuery(Name = TokenQueryParameter)] String? token,
		[FromQuery(Name = RedirectUriQueryParameter)] String? redirectUri,
		[FromServices] UserManager<User> userManager,
		HttpRequest httpRequest,
		[FromServices] ConfirmEmailEmailSender confirmEmailEmailSender)
	{
		email = WebUtility.UrlDecode(email);
		redirectUri = WebUtility.UrlDecode(redirectUri);

		var user = await userManager.FindByEmailAsync(email).ConfigureAwait(false);
		if (user is null)
		{
			// We could return NotFound, but it feels like unnecessary information.
			return RedirectOrSendNoContent(redirectUri);
		}

		if (await userManager.IsEmailConfirmedAsync(user).ConfigureAwait(false))
		{
			return RedirectOrSendNoContent(redirectUri);
		}

		if (token is null)
		{
			await confirmEmailEmailSender.SendEmailAsync(user, redirectUri, httpRequest).ConfigureAwait(false);
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

		_ = await userManager.ConfirmEmailAsync(user, token).ConfigureAwait(false);

		return RedirectOrSendNoContent(redirectUri);
	}

	private static Results<NoContent, RedirectHttpResult> RedirectOrSendNoContent(String? redirectUri)
	{
		return redirectUri is not null ? TypedResults.Redirect(redirectUri) : TypedResults.NoContent();
	}
}
