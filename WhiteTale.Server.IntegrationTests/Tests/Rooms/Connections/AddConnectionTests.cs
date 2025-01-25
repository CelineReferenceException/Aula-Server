using System.Net;
using WhiteTale.Server.Domain.Users;
using WhiteTale.Server.Features.Rooms.Connections;

namespace WhiteTale.Server.IntegrationTests.Tests.Rooms.Connections;

public sealed class AddConnectionTests
{
	[Fact]
	public async Task AddConnection_ValidOperation_ReturnsNoContent()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(AddConnection_ValidOperation_ReturnsNoContent));
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

		using var request = new HttpRequestMessage(HttpMethod.Put, $"api/v1/rooms/{firstRoomSeed.Seed.Id}/connections");
		var requestBody = new AddConnectionRequestBody
		{
			RoomId = secondRoomSeed.Seed.Id,
		};
		request.SetJsonContent(requestBody);
		request.SetAuthorization("Bearer", userCredentials.AccessToken);

		// Act
		using var response = await client.SendAsync(request);

		// Assert
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.NoContent);
	}

	[Fact]
	public async Task AddConnection_TwoTimes_ReturnsNoContent()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(AddConnection_TwoTimes_ReturnsNoContent));
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

		var requestBody = new AddConnectionRequestBody
		{
			RoomId = secondRoomSeed.Seed.Id,
		};
		using var firstRequest = new HttpRequestMessage(HttpMethod.Put, $"api/v1/rooms/{firstRoomSeed.Seed.Id}/connections");
		using var secondRequest = new HttpRequestMessage(HttpMethod.Put, $"api/v1/rooms/{firstRoomSeed.Seed.Id}/connections");
		firstRequest.SetJsonContent(requestBody);
		secondRequest.SetJsonContent(requestBody);
		firstRequest.SetAuthorization("Bearer", userCredentials.AccessToken);
		secondRequest.SetAuthorization("Bearer", userCredentials.AccessToken);

		// Act
		_ = await client.SendAsync(firstRequest);
		using var response = await client.SendAsync(secondRequest);

		// Assert
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.NoContent);
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.NoContent);
	}
}
