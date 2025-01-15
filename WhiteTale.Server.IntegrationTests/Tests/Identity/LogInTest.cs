using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using WhiteTale.Server.Domain.Users;
using WhiteTale.Server.Features.Identity;

namespace WhiteTale.Server.IntegrationTests.Tests.Identity;

public sealed class LogInTest
{
	[Fact]
	public async Task LogIn_WithValidCredentials_ReturnsOkWithAccessTokenResponse()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(LogIn_WithValidCredentials_ReturnsOkWithAccessTokenResponse));
		using var httpClient = application.CreateClient();

		var userSeed = await application.SeedUserAsync();

		var requestBody = new LogInRequestBody
		{
			UserName = userSeed.Seed.UserName,
			Password = userSeed.Seed.Password
		};

		// Act
		using var response = await httpClient.PostAsJsonAsync("api/identity/login", requestBody);

		// Assert
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.OK);
		var responseBody = await response.Content.ReadFromJsonAsync<AccessTokenResponse>();
		_ = responseBody.Should().NotBeNull();
		_ = responseBody!.TokenType.Should().Be("Bearer");
		_ = responseBody.AccessToken.Should().NotBeNullOrWhiteSpace();
		_ = responseBody.RefreshToken.Should().NotBeNullOrWhiteSpace();
		_ = responseBody.ExpiresIn.Should().BeGreaterThan(0);
	}

	[Fact]
	public async Task LogIn_WithUnknownUserName_ReturnsBadRequest()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(LogIn_WithUnknownUserName_ReturnsBadRequest));
		using var httpClient = application.CreateClient();

		var requestBody = new LogInRequestBody
		{
			UserName = "0",
			Password = "NewTestPassword1!"
		};

		// Act
		using var response = await httpClient.PostAsJsonAsync("api/identity/login", requestBody);

		// Assert
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task LogIn_WithIncorrectPassword_ReturnsBadRequest()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(LogIn_WithIncorrectPassword_ReturnsBadRequest));
		using var httpClient = application.CreateClient();

		var userSeed = await application.SeedUserAsync();

		var requestBody = new LogInRequestBody
		{
			UserName = userSeed.Seed.UserName,
			Password = "0"
		};

		// Act
		using var response = await httpClient.PostAsJsonAsync("api/identity/login", requestBody);

		// Assert
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task LogIn_WithoutNoConfirmedEmail_ReturnsForbidden()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(LogIn_WithoutNoConfirmedEmail_ReturnsForbidden));
		using var httpClient = application.CreateClient();

		var userSeed = await application.SeedUserAsync(UserSeed.Default with { EmailConfirmed = false });

		var requestBody = new LogInRequestBody
		{
			UserName = userSeed.Seed.UserName,
			Password = userSeed.Seed.Password
		};

		// Act
		using var response = await httpClient.PostAsJsonAsync("api/identity/login", requestBody);

		// Assert
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.Forbidden);
	}

	[Fact]
	public async Task LogIn_WithAnActiveLockout_ReturnsForbidden()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(LogIn_WithAnActiveLockout_ReturnsForbidden));
		using var httpClient = application.CreateClient();

		var userSeed = await application.SeedUserAsync();

		using var arrangementScope = application.Services.CreateScope();
		var arrangementUserManager = arrangementScope.ServiceProvider.GetRequiredService<UserManager<User>>();
		var userToLockout = await arrangementUserManager.FindByIdAsync(userSeed.Seed.Id.ToString());
		var availableAttempts = arrangementUserManager.Options.Lockout.MaxFailedAccessAttempts;
		for (var i = 0; i < availableAttempts; i++)
		{
			_ = await arrangementUserManager.AccessFailedAsync(userToLockout!);
		}

		var requestBody = new LogInRequestBody
		{
			UserName = userSeed.Seed.UserName,
			Password = userSeed.Seed.Password
		};

		// Act
		using var response = await httpClient.PostAsJsonAsync("api/identity/login", requestBody);

		// Assert
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.Forbidden);
	}

	[Fact]
	public async Task LogIn_WithEmptyBody_ReturnsBadRequest()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(LogIn_WithEmptyBody_ReturnsBadRequest));
		using var httpClient = application.CreateClient();

		// Act
		using var response = await httpClient.PostAsJsonAsync("api/identity/login", String.Empty);

		// Assert
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.BadRequest);
	}
}
