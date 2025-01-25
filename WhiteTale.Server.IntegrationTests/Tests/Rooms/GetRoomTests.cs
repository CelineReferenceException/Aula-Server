using System.Net;
using System.Net.Http.Json;
using WhiteTale.Server.Features.Rooms;

namespace WhiteTale.Server.IntegrationTests.Tests.Rooms;

public sealed class GetRoomTests
{
	[Fact]
	public async Task GetRoom_ValidOperation_ReturnsOkWithRoom()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(GetRoom_ValidOperation_ReturnsOkWithRoom));
		using var client = application.CreateClient();

		var userSeed = await application.SeedUserAsync();
		var userCredentials = await application.LoginUserAsync(userSeed.Seed.UserName, userSeed.Seed.Password);

		var roomSeed = await application.SeedRoomAsync();
		var room = roomSeed.Room;

		using var request = new HttpRequestMessage(HttpMethod.Get, $"api/v1/rooms/{room.Id}");
		request.SetAuthorization("Bearer", userCredentials.AccessToken);

		// Act
		using var response = await client.SendAsync(request);

		// Assert
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.OK);
		var responseBody = await response.Content.ReadFromJsonAsync<RoomData>();
		_ = responseBody.Should().NotBeNull();
		_ = responseBody!.Id.Should().Be(room.Id);
		_ = responseBody.Name.Should().Be(room.Name);
		_ = responseBody.Description.Should().Be(room.Description);
		_ = responseBody.IsEntrance.Should().Be(room.IsEntrance);
	}

	[Fact]
	public async Task GetRoom_UnknownRoom_ReturnsNotFound()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(GetRoom_UnknownRoom_ReturnsNotFound));
		using var client = application.CreateClient();

		var userSeed = await application.SeedUserAsync();
		var userCredentials = await application.LoginUserAsync(userSeed.Seed.UserName, userSeed.Seed.Password);

		using var request = new HttpRequestMessage(HttpMethod.Get, "api/v1/rooms/0");
		request.SetAuthorization("Bearer", userCredentials.AccessToken);

		// Act
		using var response = await client.SendAsync(request);

		// Assert
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.NotFound);
	}
}
