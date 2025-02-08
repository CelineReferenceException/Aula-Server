using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace WhiteTale.Server.Features.Identity.Endpoints;

internal sealed class ConfirmEmailEmailSender
{
	private readonly String _applicationName;
	private readonly IEmailSender _emailSender;
	private readonly UserManager _userManager;

	public ConfirmEmailEmailSender(
		[FromServices] UserManager userManager,
		[FromServices] IEmailSender emailSender,
		[FromServices] IOptions<ApplicationOptions> applicationOptions)
	{
		_userManager = userManager;
		_emailSender = emailSender;
		_applicationName = applicationOptions.Value.Name;
	}

	internal async Task SendEmailAsync(User user, HttpRequest httpRequest)
	{
		var email = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(user.Email));
		var confirmationToken = _userManager.GenerateEmailConfirmationToken(user);
		confirmationToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(confirmationToken));
		var confirmationUrl =
			$"{httpRequest.Scheme}://{httpRequest.Host}{httpRequest.PathBase}/{ConfirmEmail.Route}?" +
			$"{ConfirmEmail.EmailQueryParameter}={email}&" +
			$"{ConfirmEmail.TokenQueryParameter}={confirmationToken}";

		var content =
			$"""
			 <p>Hello {user.UserName}, Welcome to {_applicationName}!</p>
			 <p>To complete your registration and verify your email address, you can <a href='{confirmationUrl}'>click here</a>.
			 <p>If you didn’t sign up for {_applicationName}, you can ignore this email.</p>
			 """;
		await _emailSender.SendEmailAsync(user.Email!, "Confirm your email", content);
	}
}
