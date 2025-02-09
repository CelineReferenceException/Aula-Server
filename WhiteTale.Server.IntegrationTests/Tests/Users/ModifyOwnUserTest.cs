using System.Net;
using System.Net.Http.Json;
using WhiteTale.Server.Features;
using WhiteTale.Server.Features.Users;
using WhiteTale.Server.Features.Users.Endpoints;

namespace WhiteTale.Server.IntegrationTests.Tests.Users;

public sealed class ModifyOwnUserTest
{
	[Fact]
	public async Task ModifyOwnUser_ValidOperation_ReturnsOkWithUser()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(ModifyOwnUser_ValidOperation_ReturnsOkWithUser));
		using var httpClient = application.CreateClient();

		var userSeed = await application.SeedUserAsync();
		var credentials = await application.LoginUserAsync(userSeed.Seed.UserName, userSeed.Seed.Password);

		using var request = new HttpRequestMessage(HttpMethod.Patch, "api/v1/users/@me");
		var requestBody = new ModifyOwnUserRequestBody
		{
			DisplayName = "NewTestUser",
			Description = "NewDescription",
		};
		request.SetJsonContent(requestBody);
		request.SetAuthorization("Bearer", credentials.AccessToken);

		// Act
		using var response = await httpClient.SendAsync(request);

		// Arrange
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.OK);
		var responseBody = await response.Content.ReadFromJsonAsync<UserData>();
		_ = responseBody.Should().NotBeNull();
		_ = responseBody!.Id.Should().Be(userSeed.User.Id);
		_ = responseBody.DisplayName.Should().Be(requestBody.DisplayName);
		_ = responseBody.Description.Should().Be(requestBody.Description);
		_ = responseBody.Presence.Should().Be(userSeed.User.Presence);
		_ = responseBody.Type.Should().Be(userSeed.User.Type);
		_ = responseBody.CurrentRoomId.Should().Be(userSeed.User.CurrentRoomId);
	}

	[Fact]
	public async Task ModifyOwnUser_WithoutAuthorization_ReturnsUnauthorized()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(ModifyOwnUser_WithoutAuthorization_ReturnsUnauthorized));
		using var httpClient = application.CreateClient();

		var requestBody = new ModifyOwnUserRequestBody
		{
			DisplayName = "NewTestUser",
			Description = "NewDescription",
		};

		// Act
		using var response = await httpClient.PatchAsJsonAsync("api/v1/users/@me", requestBody);

		// Arrange
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.Unauthorized);
	}
}
