using System.Net;
using System.Net.Http.Json;
using WhiteTale.Server.Features.Rooms;

namespace WhiteTale.Server.IntegrationTests.Tests.Rooms;

public sealed class GetAllRoomsTests
{
	[Fact]
	public async Task GetAllRooms_ValidOperation_ReturnsOkWithRooms()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(GetAllRooms_ValidOperation_ReturnsOkWithRooms));
		using var client = application.CreateClient();

		var userSeed = await application.SeedUserAsync();
		var userCredentials = await application.LoginUserAsync(userSeed.Seed.UserName, userSeed.Seed.Password);

		var firstRoomSeed = await application.SeedRoomAsync(RoomSeed.Default with { Id = 1 });
		var secondRoomSeed = await application.SeedRoomAsync(RoomSeed.Default with { Id = 2 });

		using var request = new HttpRequestMessage(HttpMethod.Get, $"api/rooms");
		request.SetAuthorization("Bearer", userCredentials.AccessToken);

		// Act
		using var response = await client.SendAsync(request);

		// Assert
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.OK);
		var responseBody = await response.Content.ReadFromJsonAsync<List<RoomData>>();
		_ = responseBody.Should().NotBeNull();

		_ = responseBody![0].Id.Should().Be(firstRoomSeed.Room.Id);
		_ = responseBody[0].Name.Should().Be(firstRoomSeed.Room.Name);
		_ = responseBody[0].Description.Should().Be(firstRoomSeed.Room.Description);
		_ = responseBody[0].IsEntrance.Should().Be(firstRoomSeed.Room.IsEntrance);

		_ = responseBody![1].Id.Should().Be(secondRoomSeed.Room.Id);
		_ = responseBody[1].Name.Should().Be(secondRoomSeed.Room.Name);
		_ = responseBody[1].Description.Should().Be(secondRoomSeed.Room.Description);
		_ = responseBody[1].IsEntrance.Should().Be(secondRoomSeed.Room.IsEntrance);
	}
}
