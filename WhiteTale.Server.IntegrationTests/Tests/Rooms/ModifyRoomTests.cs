using System.Net;
using System.Net.Http.Json;
using WhiteTale.Server.Domain.Users;
using WhiteTale.Server.Features.Rooms;
using WhiteTale.Server.Features.Rooms.Endpoints;

namespace WhiteTale.Server.IntegrationTests.Tests.Rooms;

public sealed class ModifyRoomTests
{
	[Fact]
	public async Task ModifyRoom_ValidOperation_ReturnsOkWithRoom()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(ModifyRoom_ValidOperation_ReturnsOkWithRoom));
		using var client = application.CreateClient();

		var userSeed = await application.SeedUserAsync(UserSeed.Default with
		{
			Permissions = Permissions.ManageRooms,
		});
		var userCredentials = await application.LoginUserAsync(userSeed.Seed.UserName, userSeed.Seed.Password);

		var roomSeed = await application.SeedRoomAsync();

		using var request = new HttpRequestMessage(HttpMethod.Patch, $"api/v1/rooms/{roomSeed.Room.Id}");
		var requestBody = new ModifyRoomRequestBody
		{
			Name = "New Test Room",
			Description = "New Room Description",
			IsEntrance = true,
		};
		request.SetJsonContent(requestBody);
		request.SetAuthorization("Bearer", userCredentials.AccessToken);

		// Act
		using var response = await client.SendAsync(request);

		// Assert
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.OK);
		var responseBody = await response.Content.ReadFromJsonAsync<RoomData>();
		_ = responseBody.Should().NotBeNull();
		_ = responseBody!.Id.Should().Be(roomSeed.Room.Id);
		_ = responseBody!.Name.Should().Be(requestBody.Name);
		_ = responseBody.Description.Should().Be(requestBody.Description);
		_ = responseBody.IsEntrance.Should().Be(requestBody.IsEntrance.Value);
	}

	[Fact]
	public async Task ModifyRoom_InvalidArguments_ReturnsBadRequest()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(ModifyRoom_InvalidArguments_ReturnsBadRequest));
		using var client = application.CreateClient();

		var userSeed = await application.SeedUserAsync(UserSeed.Default with
		{
			Permissions = Permissions.ManageRooms,
		});
		var userCredentials = await application.LoginUserAsync(userSeed.Seed.UserName, userSeed.Seed.Password);

		var roomSeed = await application.SeedRoomAsync();

		using var request = new HttpRequestMessage(HttpMethod.Patch, $"api/v1/rooms/{roomSeed.Room.Id}");
		var requestBody = new ModifyRoomRequestBody
		{
			Name = "0",
		};
		request.SetJsonContent(requestBody);
		request.SetAuthorization("Bearer", userCredentials.AccessToken);

		// Act
		using var response = await client.SendAsync(request);

		// Assert
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.BadRequest);
	}
}
