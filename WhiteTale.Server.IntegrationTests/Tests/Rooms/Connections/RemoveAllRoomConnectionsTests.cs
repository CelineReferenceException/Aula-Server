using System.Net;
using System.Net.Http.Json;
using WhiteTale.Server.Domain.Users;

namespace WhiteTale.Server.IntegrationTests.Tests.Rooms.Connections;

public class RemoveAllRoomConnectionsTests
{
	[Fact]
	public async Task RemoveAllConnections_ValidOperation_ReturnsOkWithConnections()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(RemoveAllConnections_ValidOperation_ReturnsOkWithConnections));
		using var client = application.CreateClient();

		var userSeed = await application.SeedUserAsync(UserSeed.Default with
		{
			Permissions = Permissions.ManageRooms,
		});
		var userCredentials = await application.LoginUserAsync(userSeed.Seed.UserName, userSeed.Seed.Password);

		var roomSeed = await application.SeedRoomAsync(RoomSeed.Default with
		{
			Id = 1,
		});
		var roomConnectionSeed = await application.SeedRoomConnectionAsync(RoomConnectionSeed.Default with
		{
			SourceRoomId = roomSeed.Seed.Id,
			TargetRoomId = 2,
		});
		using var request = new HttpRequestMessage(HttpMethod.Delete,
			$"api/v1/rooms/{roomSeed.Seed.Id}/connections");
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
