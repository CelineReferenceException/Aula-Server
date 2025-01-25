using System.Net;
using WhiteTale.Server.Domain.Users;
using WhiteTale.Server.Features.Users.CurrentRoom;

namespace WhiteTale.Server.IntegrationTests.Tests.Users.CurrentRoom;

public sealed class SetCurrentRoomTests
{
	[Fact]
	public async Task SetCurrentRoom_ValidOperation_ReturnsNoContent()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(SetCurrentRoom_ValidOperation_ReturnsNoContent));
		using var httpClient = application.CreateClient();

		var executorUserSeed = await application.SeedUserAsync(UserSeed.Default with
		{
			Id = 1,
			UserName = "test_user_x",
			Email = "test_address_1@example.com",
			Permissions = Permissions.SetCurrentRoom,
		});
		var credentials = await application.LoginUserAsync(executorUserSeed.Seed.UserName, executorUserSeed.Seed.Password);

		var targetUserId = await application.SeedUserAsync(UserSeed.Default with
		{
			Id = 2,
			UserName = "test_user_y",
			Email = "test_address_2@example.com",
		});

		var roomSeed = await application.SeedRoomAsync(RoomSeed.Default with
		{
			IsEntrance = true,
		});

		using var request = new HttpRequestMessage(HttpMethod.Put, $"api/v1/users/{targetUserId.Seed.Id}/current-room");
		var requestBody = new SetCurrentRoomRequestBody
		{
			RoomId = roomSeed.Seed.Id,
		};
		request.SetJsonContent(requestBody);
		request.SetAuthorization("Bearer", credentials.AccessToken);

		// Act
		using var response = await httpClient.SendAsync(request);

		// Arrange
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.NoContent);
	}

	[Fact]
	public async Task SetCurrentRoom_UnknownRoom_ReturnsBadRequest()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(SetCurrentRoom_UnknownRoom_ReturnsBadRequest));
		using var httpClient = application.CreateClient();

		var executorUserSeed = await application.SeedUserAsync(UserSeed.Default with
		{
			Id = 1,
			UserName = "test_user_x",
			Email = "test_address_1@example.com",
			Permissions = Permissions.SetCurrentRoom,
		});
		var credentials = await application.LoginUserAsync(executorUserSeed.Seed.UserName, executorUserSeed.Seed.Password);

		var targetUserId = await application.SeedUserAsync(UserSeed.Default with
		{
			Id = 2,
			UserName = "test_user_y",
			Email = "test_address_2@example.com",
		});

		using var request = new HttpRequestMessage(HttpMethod.Put, $"api/v1/users/{targetUserId.Seed.Id}/current-room");
		var requestBody = new SetCurrentRoomRequestBody
		{
			RoomId = 0,
		};
		request.SetJsonContent(requestBody);
		request.SetAuthorization("Bearer", credentials.AccessToken);

		// Act
		using var response = await httpClient.SendAsync(request);

		// Arrange
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task SetCurrentRoom_FirstRoomNotEntrance_ReturnsBadRequest()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(SetCurrentRoom_FirstRoomNotEntrance_ReturnsBadRequest));
		using var httpClient = application.CreateClient();

		var executorUserSeed = await application.SeedUserAsync(UserSeed.Default with
		{
			Id = 1,
			UserName = "test_user_x",
			Email = "test_address_1@example.com",
			Permissions = Permissions.SetCurrentRoom,
		});
		var credentials = await application.LoginUserAsync(executorUserSeed.Seed.UserName, executorUserSeed.Seed.Password);

		var targetUserId = await application.SeedUserAsync(UserSeed.Default with
		{
			Id = 2,
			UserName = "test_user_y",
			Email = "test_address_2@example.com",
		});

		var roomSeed = await application.SeedRoomAsync();

		using var request = new HttpRequestMessage(HttpMethod.Put, $"api/v1/users/{targetUserId.Seed.Id}/set-current-room");
		var requestBody = new SetCurrentRoomRequestBody
		{
			RoomId = roomSeed.Seed.Id,
		};
		request.SetJsonContent(requestBody);
		request.SetAuthorization("Bearer", credentials.AccessToken);

		// Act
		using var response = await httpClient.SendAsync(request);

		// Arrange
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.BadRequest);
	}
}
