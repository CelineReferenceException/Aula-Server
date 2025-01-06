using System.Net;
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
		[FromServices] UserManager<User> userManager,
		[FromServices] ResetPasswordEmailSender resetPasswordEmailSender)
	{
		email = WebUtility.UrlDecode(email);
		resetUri = WebUtility.UrlDecode(resetUri);

		var user = await userManager.FindByEmailAsync(email).ConfigureAwait(false);
		if (user is null)
		{
			// We could return NotFound, but it feels like unnecessary information.
			return TypedResults.NoContent();
		}

		await resetPasswordEmailSender.SendEmailAsync(user, resetUri).ConfigureAwait(false);
		return TypedResults.NoContent();
	}
}
