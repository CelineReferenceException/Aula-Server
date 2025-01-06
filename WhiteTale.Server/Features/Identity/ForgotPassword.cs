using System.Net;
using System.Text;
using Microsoft.AspNetCore.Identity.UI.Services;
using WhiteTale.Server.Domain.Users;

namespace WhiteTale.Server.Features.Identity;

internal sealed class ForgotPassword : IEndpoint
{
	public void Build(IEndpointRouteBuilder route)
	{
		_ = route.MapPost("api/identity/forgotpassword", HandleAsync)
			.RequireRateLimiting(RateLimitPolicyNames.Global)
			.RequireRateLimiting(RateLimitPolicyNames.Strict)
			.RequireRateLimiting(RateLimitPolicyNames.NoConcurrency);

	}

	private static async Task<NoContent> HandleAsync(
		[FromQuery] String email,
		[FromQuery] String? resetUri,
		HttpRequest httpRequest,
		[FromServices] UserManager<User> userManager,
		[FromServices] IEmailSender emailSender)
	{
		email = WebUtility.UrlDecode(email);
		resetUri = WebUtility.UrlDecode(resetUri);

		var user = await userManager.FindByEmailAsync(email).ConfigureAwait(false);
		if (user is null)
		{
			// We could return NotFound, but it feels like unnecessary information.
			return TypedResults.NoContent();
		}

		var resetToken = await userManager.GeneratePasswordResetTokenAsync(user).ConfigureAwait(false);
		resetToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(resetToken));
		var content =
			$"""
			 <p>Here's your user ID and a reset token:</p>
			 <ul>
			 	<li><strong>User ID:</strong> <code>{user.Id}</code></li>
			 	<li><strong>Reset token:</strong> <code>{resetToken}</code></li>
			 </ul>
			 """;

		if (resetUri is not null)
		{
			content += $"<p>You can reset your password here: <a href='{resetUri}'>{resetUri}</a></p>";
		}

		await emailSender.SendEmailAsync(email, "Reset your password", content).ConfigureAwait(false);
		return TypedResults.NoContent();
	}
}
