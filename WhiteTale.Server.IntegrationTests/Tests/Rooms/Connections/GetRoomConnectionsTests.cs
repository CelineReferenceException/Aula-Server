using System.Net;
using System.Net.Http.Json;

namespace WhiteTale.Server.IntegrationTests.Tests.Rooms.Connections;

public sealed class GetRoomConnectionsTests
{
	[Fact]
	public async Task GetConnections_ValidOperation_ReturnsOkWithConnections()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(GetConnections_ValidOperation_ReturnsOkWithConnections));
		using var client = application.CreateClient();

		var userSeed = await application.SeedUserAsync();
		var userCredentials = await application.LoginUserAsync(userSeed.Seed.UserName, userSeed.Seed.Password);

		var roomSeed = await application.SeedRoomAsync();
		var roomConnectionSeed = await application.SeedRoomConnectionAsync(RoomConnectionSeed.Default with
		{
			SourceRoomId = roomSeed.Seed.Id,
		});
		using var request = new HttpRequestMessage(HttpMethod.Get, $"api/v1/rooms/{roomSeed.Seed.Id}/connections");
		request.SetAuthorization("Bearer", userCredentials.AccessToken);

		// Act
		using var response = await client.SendAsync(request);

		// Assert
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.OK);
		var responseBody = await response.Content.ReadFromJsonAsync<List<UInt64>>();
		_ = responseBody.Should().NotBeNullOrEmpty();
		_ = responseBody![0].Should().Be(roomConnectionSeed.Seed.TargetRoomId);
	}
}
