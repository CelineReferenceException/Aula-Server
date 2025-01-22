using System.Net;
using WhiteTale.Server.Domain.Users;
using WhiteTale.Server.Features.Users.SetCurrentRoom;

namespace WhiteTale.Server.IntegrationTests.Tests.Users.SetCurrentRoom;

public sealed class SetOwnCurrentRoomTests
{
	[Fact]
	public async Task SetOwnCurrentRoom_ValidOperation_ReturnsNoContent()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(SetOwnCurrentRoom_ValidOperation_ReturnsNoContent));
		using var httpClient = application.CreateClient();

		var userSeed = await application.SeedUserAsync(UserSeed.Default with
		{
			Permissions = Permissions.SetOwnCurrentRoom,
		});
		var credentials = await application.LoginUserAsync(userSeed.Seed.UserName, userSeed.Seed.Password);

		var roomSeed = await application.SeedRoomAsync(RoomSeed.Default with
		{
			IsEntrance = true,
		});

		using var request = new HttpRequestMessage(HttpMethod.Put, "api/users/@me/set-current-room");
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
	public async Task SetOwnCurrentRoom_UnknownRoom_ReturnsBadRequest()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(SetOwnCurrentRoom_UnknownRoom_ReturnsBadRequest));
		using var httpClient = application.CreateClient();

		var userSeed = await application.SeedUserAsync(UserSeed.Default with
		{
			Permissions = Permissions.SetOwnCurrentRoom,
		});
		var credentials = await application.LoginUserAsync(userSeed.Seed.UserName, userSeed.Seed.Password);

		using var request = new HttpRequestMessage(HttpMethod.Put, "api/users/@me/set-current-room");
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
	public async Task SetOwnCurrentRoom_FirstRoomNotEntrance_ReturnsBadRequest()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(SetOwnCurrentRoom_FirstRoomNotEntrance_ReturnsBadRequest));
		using var httpClient = application.CreateClient();

		var userSeed = await application.SeedUserAsync(UserSeed.Default with
		{
			Permissions = Permissions.SetOwnCurrentRoom,
		});
		var credentials = await application.LoginUserAsync(userSeed.Seed.UserName, userSeed.Seed.Password);

		var roomSeed = await application.SeedRoomAsync();

		using var request = new HttpRequestMessage(HttpMethod.Put, "api/users/@me/set-current-room");
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
