using System.Net;
using System.Net.Http.Json;
using WhiteTale.Server.Features.Users;

namespace WhiteTale.Server.IntegrationTests.Tests.Users;

public sealed class GetOwnUserTest
{
	[Fact]
	public async Task GetOwnUser_ValidOperation_ReturnsOkWithUser()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(GetOwnUser_ValidOperation_ReturnsOkWithUser));
		using var httpClient = application.CreateClient();

		var userSeed = await application.SeedUserAsync();
		var credentials = await application.LoginUserAsync(userSeed.Seed.UserName, userSeed.Seed.Password);

		using var request = new HttpRequestMessage(HttpMethod.Get, "api/v1/users/@me");
		request.SetAuthorization("Bearer", credentials.AccessToken);

		// Act
		using var response = await httpClient.SendAsync(request);

		// Arrange
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.OK);
		var responseBody = await response.Content.ReadFromJsonAsync<UserData>();
		_ = responseBody.Should().NotBeNull();
		_ = responseBody!.Id.Should().Be(userSeed.User!.Id);
		_ = responseBody.DisplayName.Should().Be(userSeed.User.DisplayName);
		_ = responseBody.Description.Should().BeNull(userSeed.User.Description);
		_ = responseBody.Presence.Should().Be(userSeed.User.Presence);
		_ = responseBody.OwnerType.Should().Be(userSeed.User.OwnerType);
		_ = responseBody.Permissions.Should().Be(userSeed.User.Permissions);
		_ = responseBody.CurrentRoomId.Should().Be(userSeed.User.CurrentRoomId);
	}

	[Fact]
	public async Task GetOwnUser_WithoutAuthorization_ReturnsUnauthorized()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(GetOwnUser_WithoutAuthorization_ReturnsUnauthorized));
		using var httpClient = application.CreateClient();

		// Act
		using var response = await httpClient.GetAsync("api/v1/users/@me");

		// Arrange
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.Unauthorized);
	}
}
