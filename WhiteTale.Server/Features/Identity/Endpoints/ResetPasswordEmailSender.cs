using System.Text;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace WhiteTale.Server.Features.Identity.Endpoints;

internal sealed class ResetPasswordEmailSender
{
	private readonly IEmailSender _emailSender;
	private readonly Uri? _redirectUri;
	private readonly UserManager _userManager;

	public ResetPasswordEmailSender(
		[FromServices] UserManager userManager,
		[FromServices] IEmailSender emailSender,
		[FromServices] IOptions<IdentityFeatureOptions> featureOptions)
	{
		_userManager = userManager;
		_emailSender = emailSender;
		_redirectUri = featureOptions.Value.ResetPasswordRedirectUri;
	}

	internal async Task SendEmailAsync(User user)
	{
		var resetToken = _userManager.GeneratePasswordResetToken(user);
		resetToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(resetToken));
		var content =
			$"""
			 <p>Hello! did you forget your password? Here's your user ID and a reset token:</p>
			 <ul>
			 	<li><strong>User ID:</strong> <code>{user.Id}</code></li>
			 	<li><strong>Reset token:</strong> <code>{resetToken}</code></li>
			 </ul>
			 """;

		if (_redirectUri is not null)
		{
			content += $"<p>You can reset your password by <a href='{_redirectUri}'>clicking here</a></p>";
		}

		await _emailSender.SendEmailAsync(user.Email, "Reset your password", content);
	}
}
