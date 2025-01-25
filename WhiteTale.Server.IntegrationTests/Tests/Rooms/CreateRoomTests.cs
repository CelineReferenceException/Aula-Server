using System.Net;
using System.Net.Http.Json;
using WhiteTale.Server.Domain.Users;
using WhiteTale.Server.Features.Rooms;

namespace WhiteTale.Server.IntegrationTests.Tests.Rooms;

public sealed class CreateRoomTests
{
	[Fact]
	public async Task CreateRoom_ValidOperation_ReturnsOkWithRoom()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(CreateRoom_ValidOperation_ReturnsOkWithRoom));
		using var client = application.CreateClient();

		var userSeed = await application.SeedUserAsync(UserSeed.Default with
		{
			Permissions = Permissions.ManageRooms,
		});
		var userCredentials = await application.LoginUserAsync(userSeed.Seed.UserName, userSeed.Seed.Password);

		using var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/rooms");
		var requestBody = new CreateRoomRequestBody
		{
			Name = "Test Room",
			Description = "Test Room Description",
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
		_ = responseBody!.Name.Should().Be(requestBody.Name);
		_ = responseBody.Description.Should().Be(requestBody.Description);
		_ = responseBody.IsEntrance.Should().Be(requestBody.IsEntrance.Value);
	}

	[Fact]
	public async Task CreateRoom_InvalidArguments_ReturnsBadRequest()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(CreateRoom_InvalidArguments_ReturnsBadRequest));
		using var client = application.CreateClient();

		var userSeed = await application.SeedUserAsync(UserSeed.Default with
		{
			Permissions = Permissions.ManageRooms,
		});
		var userCredentials = await application.LoginUserAsync(userSeed.Seed.UserName, userSeed.Seed.Password);

		using var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/rooms");
		var requestBody = new CreateRoomRequestBody
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
