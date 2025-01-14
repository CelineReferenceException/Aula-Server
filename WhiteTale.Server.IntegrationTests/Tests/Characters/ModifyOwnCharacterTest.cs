using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using WhiteTale.Server.Domain.Characters;
using WhiteTale.Server.Domain.Users;
using WhiteTale.Server.Features.Characters;

namespace WhiteTale.Server.IntegrationTests.Tests.Characters;

public sealed class ModifyOwnCharacterTest
{
	[Fact]
	public async Task ModifyOwnCharacter_ValidOperation_ReturnsOkWithCharacter()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(ModifyOwnCharacter_ValidOperation_ReturnsOkWithCharacter));
		using var httpClient = application.CreateClient();

		var userSeed = await application.SeedUserAsync();
		var credentials = await application.LoginUserAsync(userSeed.Seed.UserName, userSeed.Seed.Password);

		using var request = new HttpRequestMessage(HttpMethod.Patch, "api/characters/@me");
		var requestBody = new ModifyOwnCharacterRequestBody
		{
			DisplayName = "NewTestUser",
			Description = "NewDescription"
		};
		request.SetJsonContent(requestBody);
		request.SetAuthorization("Bearer", credentials.AccessToken);

		// Act
		using var response = await httpClient.SendAsync(request);

		// Arrange
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.OK);
		var responseBody = await response.Content.ReadFromJsonAsync<CharacterData>();
		_ = responseBody.Should().NotBeNull();
		_ = responseBody!.Id.Should().Be(userSeed.Character.Id);
		_ = responseBody.DisplayName.Should().Be(requestBody.DisplayName);
		_ = responseBody.Description.Should().Be(requestBody.Description);
		_ = responseBody.Presence.Should().Be(userSeed.Character.Presence);
		_ = responseBody.OwnerType.Should().Be(userSeed.Character.OwnerType);
		_ = responseBody.CurrentRoomId.Should().Be(userSeed.Character.CurrentRoomId);
	}

	[Fact]
	public async Task ModifyOwnCharacter_WithoutAuthorization_ReturnsUnauthorized()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(ModifyOwnCharacter_WithoutAuthorization_ReturnsUnauthorized));
		using var httpClient = application.CreateClient();

		var requestBody = new ModifyOwnCharacterRequestBody
		{
			DisplayName = "NewTestUser",
			Description = "NewDescription"
		};

		// Act
		using var response = await httpClient.PatchAsJsonAsync("api/characters/@me", requestBody);

		// Arrange
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.Unauthorized);
	}
}
