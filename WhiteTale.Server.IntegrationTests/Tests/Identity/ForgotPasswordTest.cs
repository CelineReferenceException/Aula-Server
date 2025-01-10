using System.Net;
using System.Net.Http.Json;

namespace WhiteTale.Server.IntegrationTests.Tests.Identity;

public sealed class ForgotPasswordTest
{
	[Fact]
	public async Task ForgotPassword_RequestResetToken_ReturnsNoContent()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(ForgotPassword_RequestResetToken_ReturnsNoContent));
		using var httpClient = application.CreateClient();

		var userSeed = new UserSeed
		{
			UserName = "test_user",
			Password = "TestPassword1!",
			Email = "test_address@example.com",
			EmailConfirmed = true
		};
		await application.SeedUserAsync(userSeed);

		// Act
		using var response = await httpClient.PostAsJsonAsync($"api/identity/forgotpassword?email={userSeed.Email}", String.Empty);

		// Assert
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.NoContent);
	}

	[Fact]
	public async Task ForgotPassword_RequestResetTokenWithUnknownEmail_ReturnsNoContent()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(ForgotPassword_RequestResetTokenWithUnknownEmail_ReturnsNoContent));
		using var httpClient = application.CreateClient();

		var email = WebUtility.UrlEncode("x@example.com");

		// Act
		using var response = await httpClient.PostAsJsonAsync($"api/identity/forgotpassword?email={email}", String.Empty);

		// Assert
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.NoContent);
	}
}
