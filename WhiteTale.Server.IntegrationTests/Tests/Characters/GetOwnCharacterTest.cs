using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using WhiteTale.Server.Domain.Characters;
using WhiteTale.Server.Domain.Users;
using WhiteTale.Server.Features.Characters;

namespace WhiteTale.Server.IntegrationTests.Tests.Characters;

public sealed class GetOwnCharacterTest
{
	[Fact]
	public async Task GetOwnCharacter_ValidOperation_ReturnsOkWithCharacter()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(GetOwnCharacter_ValidOperation_ReturnsOkWithCharacter));
		using var httpClient = application.CreateClient();

		var userSeed = new UserSeed
		{
			DisplayName = "TestUser",
			UserName = "test_user",
			Password = "TestPassword1!",
			Email = "test_address@example.com",
			EmailConfirmed = true
		};
		await application.SeedUserAsync(userSeed);

		using var arrangementScope = application.Services.CreateScope();
		var arrangementUserManager = arrangementScope.ServiceProvider.GetRequiredService<UserManager<User>>();
		var userToGet = await arrangementUserManager.FindByNameAsync(userSeed.UserName);

		var credentials = await application.LoginUserAsync(userSeed.UserName, userSeed.Password);

		using var request = new HttpRequestMessage(HttpMethod.Get, "api/characters/@me");
		request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", credentials.AccessToken);

		// Act
		using var response = await httpClient.SendAsync(request);

		// Arrange
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.OK);
		var responseBody = await response.Content.ReadFromJsonAsync<CharacterData>();
		_ = responseBody.Should().NotBeNull();
		_ = responseBody!.Id.Should().Be(userToGet!.Id);
		_ = responseBody.DisplayName.Should().Be(userSeed.DisplayName);
		_ = responseBody.Description.Should().BeNull();
		_ = responseBody.Permissions.Should().Be(0);
		_ = responseBody.Presence.Should().Be(Presence.Offline);
		_ = responseBody.OwnerType.Should().Be(CharacterOwnerType.Standard);
		_ = responseBody.CurrentRoomId.Should().BeNull();
	}

	[Fact]
	public async Task GetOwnCharacter_WithoutAuthorization_ReturnsUnauthorized()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(GetOwnCharacter_WithoutAuthorization_ReturnsUnauthorized));
		using var httpClient = application.CreateClient();

		var userSeed = new UserSeed
		{
			DisplayName = "TestUser",
			UserName = "test_user",
			Password = "TestPassword1!",
			Email = "test_address@example.com",
			EmailConfirmed = true
		};
		await application.SeedUserAsync(userSeed);

		// Act
		using var response = await httpClient.GetAsync("api/characters/@me");

		// Arrange
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.Unauthorized);
	}
}
