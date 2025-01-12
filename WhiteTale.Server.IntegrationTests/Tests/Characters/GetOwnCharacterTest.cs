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

		var userSeed = await application.SeedUserAsync();
		var credentials = await application.LoginUserAsync(userSeed.Seed.UserName, userSeed.Seed.Password);

		using var request = new HttpRequestMessage(HttpMethod.Get, "api/characters/@me");
		request.SetAuthorization("Bearer", credentials.AccessToken);

		// Act
		using var response = await httpClient.SendAsync(request);

		// Arrange
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.OK);
		var responseBody = await response.Content.ReadFromJsonAsync<CharacterData>();
		_ = responseBody.Should().NotBeNull();
		_ = responseBody!.Id.Should().Be(userSeed.Character!.Id);
		_ = responseBody.DisplayName.Should().Be(userSeed.Character.DisplayName);
		_ = responseBody.Description.Should().BeNull(userSeed.Character.Description);
		_ = responseBody.Presence.Should().Be(userSeed.Character.Presence);
		_ = responseBody.OwnerType.Should().Be(userSeed.Character.OwnerType);
		_ = responseBody.CurrentRoomId.Should().Be(userSeed.Character.CurrentRoomId);
	}

	[Fact]
	public async Task GetOwnCharacter_WithoutAuthorization_ReturnsUnauthorized()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(GetOwnCharacter_WithoutAuthorization_ReturnsUnauthorized));
		using var httpClient = application.CreateClient();

		// Act
		using var response = await httpClient.GetAsync("api/characters/@me");

		// Arrange
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.Unauthorized);
	}
}
