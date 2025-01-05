using System.Net;
using System.Text;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using WhiteTale.Server.Common.RateLimiting;
using WhiteTale.Server.Domain.Users;

namespace WhiteTale.Server.Features.Identity;

internal sealed class ConfirmEmail : IEndpoint
{
	private const String Route = "api/identity/confirmEmail";

	public void Build(IEndpointRouteBuilder route)
	{
		// Using GET allows browsers to make direct requests, such as when a user enters a URL in the address bar.
		_ = route.MapGet(Route, HandleAsync)
			.RequireRateLimiting(RateLimitPolicyNames.Global)
			.RequireRateLimiting(RateLimitPolicyNames.NoConcurrency);
	}

	private static async Task<Results<NoContent, RedirectHttpResult>> HandleAsync(
		[FromQuery] String email,
		[FromQuery] String? token,
		[FromQuery] String? redirectUri,
		[FromServices] HttpRequest httpRequest,
		[FromServices] UserManager<User> userManager,
		[FromServices] IEmailSender emailSender,
		[FromServices] IOptions<ApplicationOptions> applicationOptions,
		[FromServices] ILogger logger)
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
			var confirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(user).ConfigureAwait(false);
			confirmationToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(confirmationToken));
			var confirmationUrl =
				$"{httpRequest.Scheme}://{httpRequest.Host}{httpRequest.PathBase}/{Route}?" +
				$"{nameof(email)}={email}&" +
				$"{nameof(token)}={confirmationToken}&" +
				(redirectUri is not null ? $"{nameof(redirectUri)}={redirectUri}" : String.Empty);
			var applicationName = applicationOptions.Value.Name;

			var content =
				$"""
				 <p>Hello {user.UserName}, Welcome to {applicationName}!</p>
				 <p>To complete your registration and verify your email address, you can go to the following link:
				 <a href='{confirmationUrl}'>{confirmationUrl}</a></p>
				 <p>If you didn’t sign up for {applicationName}, you can ignore this email.</p>
				 """;
			await emailSender.SendEmailAsync(user.Email!, "Confirm this email", content).ConfigureAwait(false);
			return RedirectOrSendNoContent(redirectUri);
		}

		token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
		_ = await userManager.ConfirmEmailAsync(user, token).ConfigureAwait(false);

		return RedirectOrSendNoContent(redirectUri);
	}

	private static Results<NoContent, RedirectHttpResult> RedirectOrSendNoContent(String? redirectUri)
	{
		return redirectUri is not null ? TypedResults.Redirect(redirectUri) : TypedResults.NoContent();
	}
}
