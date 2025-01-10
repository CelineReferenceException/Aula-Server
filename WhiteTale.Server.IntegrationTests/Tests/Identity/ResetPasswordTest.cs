using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using WhiteTale.Server.Domain.Users;
using WhiteTale.Server.Features.Identity;

namespace WhiteTale.Server.IntegrationTests.Tests.Identity;

public sealed class ResetPasswordTest
{
	[Fact]
	public async Task ResetPassword_TargetUnknownUser_ReturnsBadRequest()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(ResetPassword_WithEmptyBody_ReturnsBadRequest));
		using var httpClient = application.CreateClient();

		var requestBody = new ResetPasswordRequestBody
		{
			UserId = 0,
			ResetToken = "0",
			NewPassword = "0"
		};

		// Act
		using var response = await httpClient.PostAsJsonAsync("api/identity/resetpassword", requestBody);

		// Arrange
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.NotFound);
	}

	[Fact]
	public async Task ResetPassword_WithEmptyBody_ReturnsBadRequest()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(ResetPassword_WithEmptyBody_ReturnsBadRequest));
		using var httpClient = application.CreateClient();

		// Act
		using var response = await httpClient.PostAsJsonAsync("api/identity/resetpassword", String.Empty);

		// Arrange
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task ResetPassword_ValidOperation_ReturnsNoContent()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(ResetPassword_ValidOperation_ReturnsNoContent));
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
		var resetToken = await arrangementUserManager.GeneratePasswordResetTokenAsync(userToConfirm!);

		var requestBody = new ResetPasswordRequestBody
		{
			UserId = userToConfirm!.Id,
			ResetToken = resetToken,
			NewPassword = "NewTestPassword1!"
		};

		// Act
		using var response = await httpClient.PostAsJsonAsync("api/identity/resetpassword", requestBody);

		// Assert
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.NoContent);
	}

	[Fact]
	public async Task ResetPassword_WithAnInvalidNewPassword_BadRequest()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(ResetPassword_WithAnInvalidNewPassword_BadRequest));
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
		var resetToken = await arrangementUserManager.GeneratePasswordResetTokenAsync(userToConfirm!);

		var requestBody = new ResetPasswordRequestBody
		{
			UserId = userToConfirm!.Id,
			ResetToken = resetToken,
			NewPassword = "0"
		};

		// Act
		using var response = await httpClient.PostAsJsonAsync("api/identity/resetpassword", requestBody);

		// Assert
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task ResetPassword_WithAnInvalidResetToken_BadRequest()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(ResetPassword_WithAnInvalidResetToken_BadRequest));
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
		var resetToken = await arrangementUserManager.GeneratePasswordResetTokenAsync(userToConfirm!);

		var requestBody = new ResetPasswordRequestBody
		{
			UserId = userToConfirm!.Id,
			ResetToken = resetToken,
			NewPassword = "0"
		};

		// Act
		using var response = await httpClient.PostAsJsonAsync("api/identity/resetpassword", requestBody);

		// Assert
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.BadRequest);
	}
}
