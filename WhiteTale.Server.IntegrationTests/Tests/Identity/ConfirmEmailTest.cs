using System.Net;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using WhiteTale.Server.Domain.Users;
using WhiteTale.Server.Features.Identity;

namespace WhiteTale.Server.IntegrationTests.Tests.Identity;

public sealed class ConfirmEmailTest
{
	[Fact]
	public async Task ConfirmEmail_TryToStartConfirmation_ReturnsNoContent()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(ConfirmEmail_TryToStartConfirmation_ReturnsNoContent));
		using var httpClient = application.CreateClient();

		var userSeed = new UserSeed
		{
			UserName = "test_user",
			Password = "TestPassword1!",
			Email = "test_address@example.com"
		};
		await application.SeedUserAsync(userSeed);

		var email = WebUtility.UrlEncode(userSeed.Email);

		// Act
		using var response = await httpClient.GetAsync($"api/identity/confirmEmail?email={email}");

		// Assert
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.NoContent);

		using var assertionsScope = application.Services.CreateScope();
		var assertionsUserManager = assertionsScope.ServiceProvider.GetRequiredService<UserManager<User>>();
		var userAfterRequest = await assertionsUserManager.FindByEmailAsync(userSeed.Email);

		_ = userAfterRequest!.EmailConfirmed.Should().BeFalse();
	}

	[Fact]
	public async Task ConfirmEmail_TryToStartConfirmationWithUnknownEmail_ReturnsNoContent()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(ConfirmEmail_TryToStartConfirmationWithUnknownEmail_ReturnsNoContent));
		using var httpClient = application.CreateClient();

		var email = WebUtility.UrlEncode("x@example.com");

		// Act
		using var response = await httpClient.GetAsync($"api/identity/confirmEmail?email={email}");

		// Assert
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.NoContent);
	}

	[Fact]
	public async Task ConfirmEmail_TryToConfirm_ReturnsNoContent()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(ConfirmEmail_TryToConfirm_ReturnsNoContent));
		using var httpClient = application.CreateClient();

		var userSeed = new UserSeed
		{
			UserName = "test_user",
			Password = "TestPassword1!",
			Email = "test_address@example.com"
		};
		await application.SeedUserAsync(userSeed);

		using var arrangementScope = application.Services.CreateScope();
		var arrangementUserManager = arrangementScope.ServiceProvider.GetRequiredService<UserManager<User>>();
		var userToConfirm = await arrangementUserManager.FindByEmailAsync(userSeed.Email);
		var confirmationToken = await arrangementUserManager.GenerateEmailConfirmationTokenAsync(userToConfirm!);

		var emailUrlEncoded = WebUtility.UrlEncode(userSeed.Email);
		var confirmationTokenBase64Encoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(confirmationToken));

		// Act
		using var response = await httpClient.GetAsync($"api/identity/confirmEmail" +
		                                               $"?{ConfirmEmail.EmailQueryParameter}={emailUrlEncoded}" +
		                                               $"&{ConfirmEmail.TokenQueryParameter}={confirmationTokenBase64Encoded}");

		// Assert
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.NoContent);

		using var assertionsScope = application.Services.CreateScope();
		var assertionsUserManager = assertionsScope.ServiceProvider.GetRequiredService<UserManager<User>>();
		var userAfterConfirmation = await assertionsUserManager.FindByEmailAsync(userSeed.Email);

		_ = userAfterConfirmation!.EmailConfirmed.Should().BeTrue();
	}

	[Fact]
	public async Task ConfirmEmail_TryToConfirmWithInvalidToken_ReturnsNoContent()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(ConfirmEmail_TryToConfirmWithInvalidToken_ReturnsNoContent));
		using var httpClient = application.CreateClient();

		var userSeed = new UserSeed
		{
			UserName = "test_user",
			Password = "TestPassword1!",
			Email = "test_address@example.com"
		};
		await application.SeedUserAsync(userSeed);

		var email = WebUtility.UrlEncode(userSeed.Email);

		// Act
		using var response = await httpClient.GetAsync($"api/identity/confirmEmail?email={email}&token=0");

		// Assert
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.NoContent);

		using var assertionsScope = application.Services.CreateScope();
		var assertionsUserManager = assertionsScope.ServiceProvider.GetRequiredService<UserManager<User>>();
		var userAfterConfirmation = await assertionsUserManager.FindByEmailAsync(userSeed.Email);

		_ = userAfterConfirmation!.EmailConfirmed.Should().BeFalse();
	}
}
