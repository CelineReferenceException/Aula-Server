using Aula.Server.Core.Domain.Users;
using Aula.Server.Core.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Aula.Server.Core.Features.Identity;

internal sealed class ResetPasswordEmailSender
{
	private readonly IEmailSender _emailSender;
	private readonly Uri? _redirectUri;
	private readonly TokenProvider _tokenProvider;
	private readonly UserManager _userManager;

	public ResetPasswordEmailSender(
		[FromServices] UserManager userManager,
		[FromServices] IEmailSender emailSender,
		[FromServices] IOptions<IdentityFeatureOptions> featureOptions,
		[FromServices] TokenProvider tokenProvider)
	{
		_userManager = userManager;
		_emailSender = emailSender;
		_tokenProvider = tokenProvider;
		_redirectUri = featureOptions.Value.ResetPasswordRedirectUri;
	}

	internal async Task SendEmailAsync(User user)
	{
		if (user.Email is null)
		{
			throw new ArgumentException("The user email address cannot be null.", nameof(user));
		}

		var resetToken = _userManager.GeneratePasswordResetToken(user);
		var code = _tokenProvider.CreateToken(user.Id.ToString(), resetToken);
		var content =
			$"""
			 <p>Hello! did you forget your password? Here's your reset password code: <code>{code}</code></p>
			 <p>If you didn't request a password reset, you can ignore this email.</p>
			 """;

		if (_redirectUri is not null)
		{
			content += $"<p>You can reset your password by <a href='{_redirectUri}'>clicking here</a></p>";
		}

		await _emailSender.SendEmailAsync(user.Email, "Reset your password", content);
	}
}
