using System.Text;
using Microsoft.AspNetCore.Identity.UI.Services;
using WhiteTale.Server.Domain.Users;

namespace WhiteTale.Server.Features.Identity;

internal sealed class ResetPasswordEmailSender
{
	private readonly IEmailSender _emailSender;
	private readonly UserManager<User> _userManager;

	public ResetPasswordEmailSender([FromServices] UserManager<User> userManager, [FromServices] IEmailSender emailSender)
	{
		_userManager = userManager;
		_emailSender = emailSender;
	}

	internal async Task SendEmailAsync(User user, String? resetUri)
	{
		if (user.Email is null)
		{
			return;
		}

		var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user).ConfigureAwait(false);
		resetToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(resetToken));
		var content =
			$"""
			 <p>Hello! did you forget your password? Here's your user ID and a reset token:</p>
			 <ul>
			 	<li><strong>User ID:</strong> <code>{user.Id}</code></li>
			 	<li><strong>Reset token:</strong> <code>{resetToken}</code></li>
			 </ul>
			 """;

		if (resetUri is not null)
		{
			content += $"<p>You can reset your password by <a href='{resetUri}'>clicking here</a></p>";
		}

		await _emailSender.SendEmailAsync(user.Email, "Reset your password", content).ConfigureAwait(false);
	}
}
