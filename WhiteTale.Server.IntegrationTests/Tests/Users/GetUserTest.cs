using System.Net;
using System.Net.Http.Json;
using WhiteTale.Server.Features.Users;

namespace WhiteTale.Server.IntegrationTests.Tests.Users;

public sealed class GetUserTest
{
	[Fact]
	public async Task GetUser_ValidOperation_ReturnsOkWithUser()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(GetUser_ValidOperation_ReturnsOkWithUser));
		using var httpClient = application.CreateClient();

		var userInfo = await application.SeedUserAsync();
		var credentials = await application.LoginUserAsync(userInfo.Seed.UserName, userInfo.Seed.Password);

		using var request = new HttpRequestMessage(HttpMethod.Get, $"api/users/{userInfo.Seed.Id}");
		request.SetAuthorization("Bearer", credentials.AccessToken);

		// Act
		using var response = await httpClient.SendAsync(request);

		// Arrange
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.OK);
		var responseBody = await response.Content.ReadFromJsonAsync<UserData>();
		_ = responseBody.Should().NotBeNull();
		_ = responseBody!.Id.Should().Be(userInfo.User!.Id);
		_ = responseBody.DisplayName.Should().Be(userInfo.User.DisplayName);
		_ = responseBody.Description.Should().BeNull(userInfo.User.Description);
		_ = responseBody.Presence.Should().Be(userInfo.User.Presence);
		_ = responseBody.OwnerType.Should().Be(userInfo.User.OwnerType);
		_ = responseBody.CurrentRoomId.Should().Be(userInfo.User.CurrentRoomId);
	}

	[Fact]
	public async Task GetUser_WithoutAuthorization_ReturnsUnauthorized()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(GetUser_WithoutAuthorization_ReturnsUnauthorized));
		using var httpClient = application.CreateClient();

		var userInfo = await application.SeedUserAsync();

		// Act
		using var response = await httpClient.GetAsync($"api/users/{userInfo.Seed.Id}");

		// Arrange
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.Unauthorized);
	}

	[Fact]
	public async Task GetUser_TargetUnknownCharacter_ReturnsNotFound()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(GetUser_TargetUnknownCharacter_ReturnsNotFound));
		using var httpClient = application.CreateClient();

		var userInfo = await application.SeedUserAsync();
		var credentials = await application.LoginUserAsync(userInfo.Seed.UserName, userInfo.Seed.Password);

		using var request = new HttpRequestMessage(HttpMethod.Get, "api/users/0");
		request.SetAuthorization("Bearer", credentials.AccessToken);

		// Act
		using var response = await httpClient.SendAsync(request);

		// Arrange
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.NotFound);
	}
}
