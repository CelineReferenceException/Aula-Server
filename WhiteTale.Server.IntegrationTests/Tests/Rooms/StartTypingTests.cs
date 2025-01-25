using System.Net;
using WhiteTale.Server.Domain.Users;

namespace WhiteTale.Server.IntegrationTests.Tests.Rooms;

public sealed class StartTypingTests
{
	[Fact]
	public async Task StartTyping_ValidOperation_ReturnsNoContent()
	{
		// Arrange
		await using var application = new ApplicationInstance(nameof(StartTyping_ValidOperation_ReturnsNoContent));
		using var client = application.CreateClient();

		var roomSeed = await application.SeedRoomAsync();
		var room = roomSeed.Room;

		var userSeed = await application.SeedUserAsync(UserSeed.Default with
		{
			CurrentRoomId = room.Id,
			Permissions = Permissions.SendMessages,
		});
		var userCredentials = await application.LoginUserAsync(userSeed.Seed.UserName, userSeed.Seed.Password);

		using var request = new HttpRequestMessage(HttpMethod.Post, $"api/v1/rooms/{room.Id}/typing");
		request.SetAuthorization("Bearer", userCredentials.AccessToken);

		// Act
		using var response = await client.SendAsync(request);

		// Assert
		_ = await response.EnsureStatusCodeAsync(HttpStatusCode.NoContent);
	}
}
