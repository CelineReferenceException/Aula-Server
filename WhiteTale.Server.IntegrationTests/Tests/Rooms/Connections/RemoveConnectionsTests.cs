using System.Net;
using WhiteTale.Server.Domain.Users;

namespace WhiteTale.Server.IntegrationTests.Tests.Rooms.Connections;

public sealed class RemoveConnectionsTests
{
	[Fact]
	public async Task RemoveConnection_ValidOperation_ReturnsNoContent()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(RemoveConnection_ValidOperation_ReturnsNoContent));
		using var client = application.CreateClient();

		var userSeed = await application.SeedUserAsync(UserSeed.Default with
		{
			Permissions = Permissions.ManageRooms,
		});
		var userCredentials = await application.LoginUserAsync(userSeed.Seed.UserName, userSeed.Seed.Password);

		var firstRoomSeed = await application.SeedRoomAsync(RoomSeed.Default with
		{
			Id = 1,
		});
		var secondRoomSeed = await application.SeedRoomAsync(RoomSeed.Default with
		{
			Id = 2,
		});
		var roomConnectionSeed = await application.SeedRoomConnectionAsync(RoomConnectionSeed.Default with
		{
			SourceRoomId = firstRoomSeed.Seed.Id,
			TargetRoomId = secondRoomSeed.Seed.Id,
		});
		using var request = new HttpRequestMessage(HttpMethod.Delete,
			$"api/v1/rooms/{firstRoomSeed.Seed.Id}/connections/{roomConnectionSeed.Seed.TargetRoomId}");
		request.SetAuthorization("Bearer", userCredentials.AccessToken);

		// Act
		using var response = await client.SendAsync(request);

		// Assert
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.NoContent);
	}
}
