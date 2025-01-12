using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using WhiteTale.Server.Domain.Characters;
using WhiteTale.Server.Domain.Users;
using WhiteTale.Server.Features.Characters;

namespace WhiteTale.Server.IntegrationTests.Tests.Characters;

// TODO: Do a commit updating things related to user seeding, then do another commit doing a general cleanup,
//       after that make a commit updating the Domain.Rooms.Room.Description to be nullable and commit the RoomHelper & related.
//       (do not commit Room related tests yet)
public sealed class GetCharacterTest
{
	[Fact]
	public async Task GetCharacter_ValidOperation_ReturnsOkWithCharacter()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(GetCharacter_ValidOperation_ReturnsOkWithCharacter));
		using var httpClient = application.CreateClient();

		var userInfo = await application.SeedUserAsync();
		var credentials = await application.LoginUserAsync(userInfo.Seed.UserName, userInfo.Seed.Password);

		using var request = new HttpRequestMessage(HttpMethod.Get, $"api/characters/{userInfo.Seed.Id}");
		request.SetAuthorization("Bearer", credentials.AccessToken);

		// Act
		using var response = await httpClient.SendAsync(request);

		// Arrange
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.OK);
		var responseBody = await response.Content.ReadFromJsonAsync<CharacterData>();
		_ = responseBody.Should().NotBeNull();
		_ = responseBody!.Id.Should().Be(userInfo.Character!.Id);
		_ = responseBody.DisplayName.Should().Be(userInfo.Character.DisplayName);
		_ = responseBody.Description.Should().BeNull(userInfo.Character.Description);
		_ = responseBody.Presence.Should().Be(userInfo.Character.Presence);
		_ = responseBody.OwnerType.Should().Be(userInfo.Character.OwnerType);
		_ = responseBody.CurrentRoomId.Should().Be(userInfo.Character.CurrentRoomId);
	}

	[Fact]
	public async Task GetCharacter_WithoutAuthorization_ReturnsUnauthorized()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(GetCharacter_WithoutAuthorization_ReturnsUnauthorized));
		using var httpClient = application.CreateClient();

		var userInfo = await application.SeedUserAsync();

		// Act
		using var response = await httpClient.GetAsync($"api/characters/{userInfo.Seed.Id}");

		// Arrange
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.Unauthorized);
	}

	[Fact]
	public async Task GetCharacter_TargetUnknownCharacter_ReturnsNotFound()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(GetCharacter_TargetUnknownCharacter_ReturnsNotFound));
		using var httpClient = application.CreateClient();

		var userInfo = await application.SeedUserAsync();
		var credentials = await application.LoginUserAsync(userInfo.Seed.UserName, userInfo.Seed.Password);

		using var request = new HttpRequestMessage(HttpMethod.Get, "api/characters/1");
		request.SetAuthorization("Bearer", credentials.AccessToken);

		// Act
		using var response = await httpClient.SendAsync(request);

		// Arrange
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.NotFound);
	}
}
