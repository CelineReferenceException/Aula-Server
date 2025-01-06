using System.Text;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using WhiteTale.Server.Domain.Users;

namespace WhiteTale.Server.Features.Identity;

internal sealed class ConfirmEmailEmailSender
{
	private readonly IOptions<ApplicationOptions> _applicationOptions;
	private readonly IEmailSender _emailSender;
	private readonly UserManager<User> _userManager;

	public ConfirmEmailEmailSender(
		[FromServices] UserManager<User> userManager,
		[FromServices] IEmailSender emailSender,
		[FromServices] IOptions<ApplicationOptions> applicationOptions)
	{
		_userManager = userManager;
		_emailSender = emailSender;
		_applicationOptions = applicationOptions;
	}

	internal async Task SendEmailAsync(User user, String? redirectUri, HttpRequest httpRequest)
	{
		var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user).ConfigureAwait(false);
		confirmationToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(confirmationToken));
		var confirmationUrl =
			$"{httpRequest.Scheme}://{httpRequest.Host}{httpRequest.PathBase}/{ConfirmEmail.Route}?" +
			$"{ConfirmEmail.EmailQueryParameterName}={user.Email}&" +
			$"{ConfirmEmail.TokenQueryParameterName}={confirmationToken}&" +
			(redirectUri is not null ? $"{ConfirmEmail.RedirectUriQueryParameterName}={redirectUri}" : String.Empty);
		var applicationName = _applicationOptions.Value.Name;

		var content =
			$"""
			 <p>Hello {user.UserName}, Welcome to {applicationName}!</p>
			 <p>To complete your registration and verify your email address, you can <a href='{confirmationUrl}'>click here</a>.
			 <p>If you didn’t sign up for {applicationName}, you can ignore this email.</p>
			 """;
		await _emailSender.SendEmailAsync(user.Email!, "Confirm your email", content).ConfigureAwait(false);
	}
}
